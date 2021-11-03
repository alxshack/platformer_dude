using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// @todo сделать абстрактный класс, экстендить из него. HealthBar тоже
public class ManaBar : MonoBehaviour
{

    public Slider slider;
    public Image fill;
    
    public void SetMaxMana(int mana)
    {
        slider.maxValue = mana;
    }
    
    public void SetMana(int mana)
    {
        slider.value = mana;
    }
}
