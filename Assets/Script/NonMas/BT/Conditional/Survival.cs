using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class Survival : Conditional
{
	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}	
}