using UnityEngine;
using Platformer.Core;
using Platformer.KeyBinding;

namespace Platformer.Player
{
    /// <summary>
    /// Translates the player's currently allocated keys into motor commands and
    /// interaction requests. All input goes through KeyBindingManager, so remapping or
    /// blocking a key changes behaviour here automatically with no per-feature
    /// special-casing. Yields to GameplayInputGate while a modal UI (key-remap menu,
    /// dialogue) is open.
    /// </summary>
    [RequireComponent(typeof(PlayerMotor2D))]
    public class PlayerController : MonoBehaviour
    {
        private PlayerMotor2D motor;
        private PlayerInteractor interactor;

        private void Awake()
        {
            motor = GetComponent<PlayerMotor2D>();
            interactor = GetComponent<PlayerInteractor>();
        }

        private void Update()
        {
            if (GameplayInputGate.IsLocked)
            {
                motor.Move(0f);
                return;
            }

            var bindings = KeyBindingManager.Instance;
            if (bindings == null) return;

            float horizontal = 0f;
            if (bindings.IsActionPressed(GameAction.MoveLeft)) horizontal -= 1f;
            if (bindings.IsActionPressed(GameAction.MoveRight)) horizontal += 1f;
            motor.Move(horizontal);

            if (bindings.IsActionDown(GameAction.Jump)) motor.Jump();
            if (bindings.IsActionDown(GameAction.Interact)) interactor?.InteractWithCurrent();
        }
    }
}
