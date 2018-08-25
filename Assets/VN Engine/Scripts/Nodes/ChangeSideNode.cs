using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VNEngine
{
    public class ChangeSideNode : Node
    {
        public string actor;
        public Actor_Positions destination;


        public override void Run_Node()
        {
            if (string.IsNullOrEmpty(actor))
            {
                Debug.LogError("Actor " + actor + " name is null or empty", this.gameObject);
            }
            // Check if the actor is already present
            else if (ActorManager.Is_Actor_On_Scene(actor))
            {
                // Actor is already on the scene
                Actor actor_script = ActorManager.Get_Actor(actor);
                actor_script.position = destination;

                ActorManager.Remove_Actor_From_Positions_Lists(actor_script);
                ActorManager.Add_Actor_To(actor_script, destination);
            }
            else
                Debug.LogError("Actor " + actor + " is not on the scene. Use EnterActorNode to place them on the scene", this.gameObject);

            Finish_Node();
        }


        public override void Button_Pressed()
        {

        }


        public override void Finish_Node()
        {
            base.Finish_Node();
        }
    }
}