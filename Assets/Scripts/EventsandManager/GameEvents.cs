using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents 
{
    //Pressure Meter Events
    public static Action<float, float> OnPressureChanged;
    
    //Event For Phase
    public static Action<int> OnPhaseChanged;

    //int 0 = Good 1 = Average 2 = Bad
    public static Action<int> OnActionPerformed;

    public static void RaisePressureChanged(float current , float max) =>OnPressureChanged?.Invoke(current, max);
    public static void RaisePhaseChanged(int phase) => OnPhaseChanged?.Invoke(phase);
    public static void RaiseActionPerformed(int actionQuality) => OnActionPerformed?.Invoke(actionQuality);
    

}
