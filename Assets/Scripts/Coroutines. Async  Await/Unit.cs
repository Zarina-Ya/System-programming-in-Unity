using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    int _health;
    int _countHealing = 5;
    int _maxHealth = 100;
    float _time = 0.5f; 
    bool _isHealing = false;
    [SerializeField] TMPro.TMP_Text text;
    public void ReceiveHealing()
    {
        if (_health != _maxHealth && !_isHealing)
        {
            _isHealing = true;

            var cureThreeSeconds = false;
            if (Random.Range(0f, 1.0f) > 0.5f)
                cureThreeSeconds = true;

            Debug.Log((cureThreeSeconds) ? "юнит получал исцеление пока количество жизней не станет равным 100" :
                "юнит получал исцеление5 жизней каждые полсекунды в течение 3 секунд");

            StartCoroutine(HealingUnit(cureThreeSeconds));
        }
        else Debug.Log("Unit have _maxHealth");
    }

    private void Update()
    {
        text.text = _health.ToString();
        Debug.Log(_isHealing);
    }
    void Start()
    {
        _health = Random.Range(0, 101);
        Debug.Log(_health);
        ReceiveHealing();
    }

    IEnumerator HealingUnit(bool value)
    {
        if (value)
        {
            while (_health < _maxHealth)
               yield return StartCoroutine(HealingUnitHalfSecond());
            
        }
        else
        {
            bool burning = true;
            float timer = 0;
            while (burning && _health < _maxHealth)
            {
                yield return StartCoroutine(HealingUnitHalfSecond());
                timer += _time;
                if (timer >= 3)
                    burning = false;
                
            }
        }
        _isHealing = false;
    }

    private IEnumerator HealingUnitHalfSecond()
    {
        yield return new WaitForSeconds(_time);
        _health += _countHealing;
        CheckMaxHealth();
    }
    private void CheckMaxHealth()
    {
        if (_health > _maxHealth)
            _health = _maxHealth;
    }
}
