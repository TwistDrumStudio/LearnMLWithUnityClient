using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace VNEngine
{
	public class PlayActorAnimationNode : Node
	{
		public string actor_name;
		public string animation_name;


		public override void Run_Node()
		{
			Actor actor = ActorManager.Get_Actor(actor_name);
			// Can only change the animation of an actor if they're already on the scene
			if (actor != null)
			{
				actor.Play_Animation(animation_name);
			} 

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