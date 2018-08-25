using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VNEngine
{
    // Used by SaveFile.cs
    // SaveFile searches the scene for FeatureToSave components, and saves the path to the Nodes that last accessed these features to save
    // When the game is loaded from SaveFile, each Node referenced by this class will be executed to recreate the scene
    public class FeatureToSave : MonoBehaviour
    {
        public Node Type_of_Node_to_Execute;

        public string Node_to_Execute;  // Path to the node that last executed on this feature. This is recorded in the savefile so the same nodes can be executed again upon loading


        public void SetFeature(Node Node_that_executed_this)
        {
            Type_of_Node_to_Execute = Node_that_executed_this;
            Node_to_Execute = SaveManager.GetGameObjectPath(Node_that_executed_this.transform);
        }
    }
}