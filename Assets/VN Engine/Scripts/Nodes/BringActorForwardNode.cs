using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace VNEngine
{
    // Brings the actor to the front of the list of actors
    public class BringActorForwardNode : Node
    {
        public string actor_name;   // Actor to bring forward


        public override void Run_Node()
        {
            ActorManager.Bring_Actor_To_Front(ActorManager.Get_Actor(actor_name));

            Finish_Node();
        }


        public override void Finish_Node()
        {
            StopAllCoroutines();

            base.Finish_Node();
        }
    }
}