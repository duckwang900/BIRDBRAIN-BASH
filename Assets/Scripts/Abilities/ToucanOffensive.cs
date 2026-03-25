using System.Collections;
using UnityEngine;

public class ToucanOffensive : BirdAbility
{
    [SerializeField]
    public float cooldown = 10f; // Cooldown in seconds (TBA)
    
    // When true the next spike by this character will be unblockable
    [HideInInspector]
    public bool abilityActive = false;
    private bool onCooldown = false;

    // Activate the ability: next spike becomes unblockable
    public void TouCanDoIt()
    {
        if (!canUseAbilities()) {
            Debug.Log("Ability has been disabled by the crow :(");
            return;
        }
        if (onCooldown)
        {
            Debug.Log("Toucan Offense on cooldown");
            return;
        }

        abilityActive = true;
        Debug.Log("Toucan offensive ability activated: next spike is unblockable");

        StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }
}
