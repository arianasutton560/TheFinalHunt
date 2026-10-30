
using UnityEngine;
using UnityEngine.InputSystem;

public class TorchEquip : MonoBehaviour
{
    public Animator torchAnimator;
    public Light torchLight;

    private bool isEquipped = false;

    void Start()
    {
        // Keep the torch light off initially.
        if (torchLight != null)
        {
            torchLight.enabled = false;
        }
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame && !isEquipped)
        {
            EquipTorch();
        }
    }

    void EquipTorch()
    {
        isEquipped = true;
        torchAnimator.SetTrigger("Equip");

        if (torchLight != null)
        {
            torchLight.enabled = true;
        }
    }
}

