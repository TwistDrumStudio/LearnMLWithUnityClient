using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace VNEngine
{
    // Darkens the given actor
    public class DarkenActorNode : Node
    {
        public string actor_name;   // Actor to darken

        public override void Run_Node()
        {
            ActorManager.Get_Actor(actor_name).Darken();

            Finish_Node();
        }


        public override void Finish_Node()
        {
            StopAllCoroutines();

            base.Finish_Node();
        }
    }
}