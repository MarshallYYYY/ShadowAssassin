using UnityEngine;
using UnityEngine.UI;

public class CharacterInformationPage : MonoBehaviour
{
    [SerializeField] private Text level;
    [SerializeField] private Text exp;
    [SerializeField] private Text hp;
    [SerializeField] private Text attack;
    [SerializeField] private Text defense;
    [SerializeField] private Text criticalRate;
    [SerializeField] private Text dodgeRate;
    void OnEnable()
    {
        PlayerData data = PersistentService.Instance.GetPlayerData();
        level.text = data.Level.ToString();
        // TODO
        exp.text = data.Exp.ToString();
        hp.text = data.HP.ToString();
        attack.text = data.Attack.ToString();
        defense.text = data.Defense.ToString();
        criticalRate.text = $"{data.CriticalRate}%";
        dodgeRate.text = $"{data.DodgeRate}%";
    }
}
