using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPhaseSettings", menuName = "Shift25/PhaseSettings")]
public class PhaseSettings : ScriptableObject
{
    [Header("Phase Progression")]
    public int phaseNumber;
    public float durationInSeconds; // Duration of the phase in seconds

    [Header("Difficulty Modifiers")]
    public float pressureTickRate; // Rate at which pressure increases
    public float customerSpawnInterval; // Interval between customer spawns

    [Header("Surrealism Level")]
    public float glitchIntensity; // Intensity of glitch effects
}
