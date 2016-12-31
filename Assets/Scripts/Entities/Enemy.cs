using UnityEngine;
using System.Collections;

namespace Completed
{
	public class Enemy : MovingEntity
	{
		public int damage;	
		
		protected override void Start ()
		{

			GameManager.instance.AddEnemyToList (this);
			base.Start ();
		}	
	}
}
