using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace VNEngine
{
    // This class is created everytime the save button is clicked.
    // A list of these SaveFiles is saved by SaveManager to the user's hard drive, and loaded when needed
    [System.Serializable]
    public class SaveFile
    {
        public string current_scene;

        // Current conversation and node
        public string current_conv;
        int current_conv_node;

        // Date saved
        public DateTime time_saved;
        // Time played, in seconds
        public float time_played;

        public string log_text;
        public Dictionary<string, string> log_categories;

        // Save all the actors on the scene
        List<string> left_actors = new List<string>();
        List<string> right_actors = new List<string>();
        List<string> center_actors = new List<string>();
        List<string> flipped_actors = new List<string>();   // List of Actors who have a - (negative) x scale

        // Keep track of which conversations are still on the scene (delete any conversations that have been deleted)
        public List<string> remaining_conversations = new List<string>();

        // List is generated by SaveFile finding all FeatureToSave components present
        // List comprises of paths of Nodes that will be executed upon load
        // Add the FeatureToSave script to any object generated by a Node that needs to saved, then have that Node populate the fields in the FeatureToSave (specifically the path to that Node, see StatcImageNode.cs)
        public List<string> Nodes_to_Execute_on_Load = new List<string>();

        // Saved current stats
        public Dictionary<string, float> saved_numbered_stats;
        public Dictionary<string, bool> saved_boolean_stats;
        public Dictionary<string, string> saved_string_stats;
        public List<string> saved_items;


        // Do not change
        const string feature_save_separation_character = ";;;;";



        public SaveFile()
        {

        }



        // Loads a scene and sets the settings of this save file to the game
        // Must be called with a monobehaviour with StartCoroutine(Load())
        // Modify this method to add in your own custom loading code
        public IEnumerator Load()
        {
            Debug.Log("Loading..." + current_conv_node);

            StatsManager.Clear_All_Stats();

            string active_scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            // UI is unresponsive if there are 2 event systems
            if (UnityEngine.EventSystems.EventSystem.current.gameObject)
                GameObject.Destroy(UnityEngine.EventSystems.EventSystem.current.gameObject);

            // Load items
            StatsManager.items = saved_items;

            VNSceneManager.scene_manager = null;

            // Load the scene that was saved
            UnityEngine.SceneManagement.SceneManager.LoadScene(current_scene, UnityEngine.SceneManagement.LoadSceneMode.Additive);

            // Must wait 1 frame before initializing the new scene
            yield return null;

            // Unpause in case the game was paused before loading
            Pause.pause.ResumeGame();

            // Stop the default things from happening
            VNSceneManager.scene_manager.starting_conversation = null;
            ConversationManager cur_conv = GameObject.Find(current_conv).GetComponent<ConversationManager>();

            // Set log
            VNSceneManager.scene_manager.Add_To_Log("", log_text);

            // Set play time
            VNSceneManager.scene_manager.play_time += time_played;

            ActorManager.actors_on_scene = new List<Actor>();
            ActorManager.exiting_actors = new List<Actor>();
            ActorManager.left_actors = new List<Actor>();
            ActorManager.right_actors = new List<Actor>();
            ActorManager.center_actors = new List<Actor>();
            /*
            // Load the actors on the scene
            foreach (string a in left_actors)
            {
                ActorManager.Instantiate_Actor(a, Actor_Positions.LEFT);
            }
            foreach (string a in right_actors)
            {
                ActorManager.Instantiate_Actor(a, Actor_Positions.RIGHT);
            }
            foreach (string a in center_actors)
            {
                ActorManager.Instantiate_Actor(a, Actor_Positions.CENTER);
            }
            */
            // Execute any Nodes that need to executed to create the scene (StaticImageNodes, SetBackground, SetMusicNode, ChangeActorImage, etc)
            foreach (string node_path in Nodes_to_Execute_on_Load)
            {
                // Figure out what type of Node we're dealing with
                string[] node_parts = node_path.Split(new string[] { feature_save_separation_character }, StringSplitOptions.None);

                // We can figure out which node is executed by the Node component embedded in the string
                if (node_parts.Length == 2)
                {
                    GameObject go = GameObject.Find(node_parts[1]);
                    if (go == null)
                    {
                        Debug.LogError("Load: Could not find node to execute; " + node_path);
                        continue;
                    }

                    Node n = (Node)go.GetComponent(node_parts[0]);
                    if (n != null)
                    {
                        n.executed_from_load = true;
                        n.Run_Node();
                    }
                    else
                        Debug.LogError("Load: Gameobject did not have attached node to execute; " + node_path);
                }
                // Can't figure out which node type this is, simply execute the first node on the found gameobject
                else if (node_parts.Length == 1)
                {
                    GameObject go = GameObject.Find(node_path);
                    if (go == null)
                    {
                        Debug.LogError("Load: Could not find node to execute; " + node_path);
                        continue;
                    }

                    Node n = go.GetComponent<Node>();
                    if (n != null)
                    {
                        n.executed_from_load = true;
                        n.Run_Node();
                    }
                    else
                        Debug.LogError("Load: Gameobject did not have attached node to execute; " + node_path);
                }
            }


            // Delete conversations not present in our saved conversations
            ConversationManager[] convs = (ConversationManager[])UnityEngine.Object.FindObjectsOfType(typeof(ConversationManager)) as ConversationManager[];
            foreach (ConversationManager c in convs)
            {
                // Find all conversations in our freshly loaded scene, check if we should keep or delete these conversations
                string name = (c.name);

                // Record the full name of the object (including its parents)
                Transform parent = c.transform.parent;
                while (parent != null)
                {
                    name = name.Insert(0, parent.name + "/");
                    parent = parent.parent;
                }


                bool delete_conv = true;
                // Check against saved conversations
                foreach (string s in remaining_conversations)
                {
                    if (name == s)
                    {
                        delete_conv = false;
                        break;
                    }
                }

                if (delete_conv)
                    GameObject.Destroy(c.gameObject);
            }

            // Load stats
            StatsManager.boolean_stats = saved_boolean_stats;
            StatsManager.numbered_stats = saved_numbered_stats;
            StatsManager.string_stats = saved_string_stats;
            StatsManager.items = saved_items;
            StatsManager.Print_All_Stats();

            // Load log text
            VNSceneManager.text_logs  = log_categories;
            VNSceneManager.scene_manager.Conversation_log = log_text;



            //<< MODIFY THIS SECTION TO LOAD THINGS SPECIFIC TO YOUR GAME >>//







            //<< MODIFY THE ABOVE SECTION TO LOAD THINGS SPECIFIC TO YOUR GAME >>//


            // Start the conversation
            cur_conv.Start_Conversation_Partway_Through(current_conv_node);

            yield return 0;

            // Flip any actors that are meant to be flipped
            foreach (string a in flipped_actors)
            {
                Actor actor = ActorManager.Get_Actor(a);
                if (a != null)
                    actor.transform.localScale = new Vector3(-actor.transform.localScale.x, actor.transform.localScale.y, actor.transform.localScale.z);
            }

            UnityEngine.SceneManagement.SceneManager.SetActiveScene(UnityEngine.SceneManagement.SceneManager.GetSceneByName(current_scene));
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(active_scene);
            AudioListener.pause = false;

            yield return 0;
        }




        // Set save information
        public void Save()
        {
            // Record game stats we must save (listed at top of file)
            log_text = UIManager.ui_manager.log_text.text;
            current_scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            //current_conversation = VNSceneManager.current_conversation;
            current_conv = SaveManager.GetGameObjectPath(VNSceneManager.current_conversation.transform);
            current_conv_node = VNSceneManager.current_conversation.cur_node;

            time_saved = DateTime.Now;
            time_played = VNSceneManager.scene_manager.play_time;

            //bg_music = AudioManager.audio_manager.background_music_audio_source.clip.;

            // Save the actors on the scene
            /*
            foreach (Actor a in ActorManager.left_actors)
            {
                left_actors.Add(a.actor_name);

                // Check if they have been flipped to face the other way
                if (Mathf.Sign(a.transform.localScale.x) == -1f)
                    flipped_actors.Add(a.actor_name);
            }
            foreach (Actor a in ActorManager.right_actors)
            {
                right_actors.Add(a.actor_name);

                // Check if they have been flipped to face the other way
                if (Mathf.Sign(a.transform.localScale.x) == -1f)
                    flipped_actors.Add(a.actor_name);
            }
            foreach (Actor a in ActorManager.center_actors)
            {
                center_actors.Add(a.actor_name);

                // Check if they have been flipped to face the other way
                if (Mathf.Sign(a.transform.localScale.x) == -1f)
                    flipped_actors.Add(a.actor_name);
            }*/


            // Record all remaining conversations (deleted ones will not be recorded)
            ConversationManager[] convs = (ConversationManager[])UnityEngine.Object.FindObjectsOfType(typeof(ConversationManager)) as ConversationManager[];
            foreach (ConversationManager c in convs)
            {
                remaining_conversations.Add(SaveManager.GetGameObjectPath(c.transform));
            }

            // Save stats
            saved_boolean_stats = StatsManager.boolean_stats;
            saved_numbered_stats = StatsManager.numbered_stats;
            saved_string_stats = StatsManager.string_stats;
            saved_items = StatsManager.items;


            // Features to save, like static images, background and foreground
            if (UIManager.ui_manager.canvas != null)
            {
                FeatureToSave[] features = UIManager.ui_manager.canvas.GetComponentsInChildren<FeatureToSave>();
                foreach (FeatureToSave f in features)
                {
                    Nodes_to_Execute_on_Load.Add(f.Type_of_Node_to_Execute.GetType() + feature_save_separation_character + f.Node_to_Execute);
                    Debug.Log(f.Node_to_Execute);
                }
            }
            else
                Debug.LogError("UIManager.ui_manager.canvas is not set. Features to save not saved");

            // Save our log text
            log_categories = VNSceneManager.text_logs;
            log_text = VNSceneManager.scene_manager.Conversation_log;

            //<< MODIFY THIS SECTION TO SAVE THINGS SPECIFIC TO YOUR GAME >>//











            //<< MODIFY THE ABOVE SECTION TO SAVE THINGS SPECIFIC TO YOUR GAME >>//




            SaveManager.AddNewSave(this);
        }
    }
}