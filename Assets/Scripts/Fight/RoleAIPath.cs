using UnityEngine;
using System.Collections;
using Pathfinding.RVO;

/** AI controller specifically made for the spider robot.
	 * The spider robot (or mine-bot) which is got from the Unity Example Project
	 * can have this script attached to be able to pathfind around with animations working properly.\n
	 * This script should be attached to a parent GameObject however since the original bot has Z+ as up.
	 * This component requires Z+ to be forward and Y+ to be up.\n
	 *
	 * It overrides the AIPath class, see that class's documentation for more information on most variables.\n
	 * Animation is handled by this component. The Animation component refered to in #anim should have animations named "awake" and "forward".
	 * The forward animation will have it's speed modified by the velocity and scaled by #animationSpeed to adjust it to look good.
	 * The awake animation will only be sampled at the end frame and will not play.\n
	 * When the end of path is reached, if the #endOfPathEffect is not null, it will be instantiated at the current position. However a check will be
	 * done so that it won't spawn effects too close to the previous spawn-point.
	 * \shadowimage{mine-bot.png}
	 *
	 * \note This script assumes Y is up and that character movement is mostly on the XZ plane.
	 */
[RequireComponent(typeof(Seeker))]
[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_mine_bot_a_i.php")]
public class RoleAIPath : AIPath 
{
	/** Minimum velocity for moving */
	public float sleepVelocity = 0.4F;

	/** Point for the last spawn of #endOfPathEffect */
	protected Vector3 lastTarget;

	public override void OnTargetReached () {
		/*if (Vector3.Distance(tr.position, lastTarget) > 1) 
		{
			lastTarget = tr.position;
		}*/
	}

	public override Vector3 GetFeetPosition () {
		return tr.position;
	}

	protected new void Update () {
		//Get velocity in world-space
		Vector3 velocity;

		if (canMove) {
			//Calculate desired velocity
			Vector3 dir = CalculateVelocity(GetFeetPosition());

			//Rotate towards targetDirection (filled in by CalculateVelocity)
			RotateTowards(targetDirection);
			dir.y = 0;
			if (dir.sqrMagnitude > sleepVelocity*sleepVelocity) {
				//If the velocity is large enough, move
			} else {
				//Otherwise, just stand still (this ensures gravity is applied)
				dir = Vector3.zero;
			}

			if (rvoController != null) {
				rvoController.Move(dir);
				velocity = rvoController.velocity;
			} else
				if (controller != null) {
					controller.SimpleMove(dir);
					velocity = controller.velocity;
				} else {
					Debug.LogWarning("No NavmeshController or CharacterController attached to GameObject");
					velocity = Vector3.zero;
				}
		} else {
			velocity = Vector3.zero;
		}

		//Animation
	}
}
