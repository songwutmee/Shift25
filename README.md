<div align="center">

# Shift25

*A psychological first-person simulation that explores the existential dread of modern labor. Set in a convenience store floating in a void, players enter "Shift 25"—a non-existent hour representing unending, unpaid labor and corporate oppression.*

[![Shift25 Preview](https://img.youtube.com/vi/fuZ6HrthnIM/maxresdefault.jpg)](https://youtu.be/fuZ6HrthnIM?si=c62dvzv-crYu7I1O)

**[  Status: Work in Progress / Mini Thesis ]**

</div>

###  Project Overview
**Shift25** is an experimental project built to reflect the burnout and systemic pressure of blue-collar service work. Instead of the clock hitting 00:00, it moves to 25:00. You are trapped in a loop of repetitive tasks—scanning items, microwaving food, and managing impatient customers—all while floating in a cosmic void. 

The game uses a **PSX-inspired visual style** with mixed-media elements to create an uncomfortable, surreal atmosphere.

---

###  Technical Approach & Architecture
Since this is a **Mini Thesis** project, I focused on building a scalable and professional architecture that can handle complex, overlapping gameplay systems.

- **Asynchronous Task Management (UniTask):** 
I used **UniTask** to handle the game's multi-tasking flow. This allows the microwave to count down and NPCs to move through queues in the background without blocking the main game logic. It makes the code much cleaner than using standard Coroutines and prevents memory leaks.

- **Event-Driven Decoupling (Observer Pattern):** 
To keep the code organized, I built a central `GameEvents` system. When a player scans an item or the "Pressure" increases, the UI and Sound systems react automatically through events. This means I can add new features (like screen glitches or debt notification sounds) without having to change the core gameplay scripts.

- **Finite State Machine (FSM):** 
NPCs and the Player are governed by a State Pattern. Customers have clear logic for `Browsing`, `Queueing`, and `Scanning`. This prevents bugs like customers walking away while being served and makes the AI behavior feel more intentional and solid.

- **Data-Driven Progression:** 
All balance values—customer spawn rates, item scan times, and the length of each game phase—are stored in **ScriptableObjects**. This allows me to tune the 60-minute gameplay experience entirely within the Unity Inspector without touching the code, which is a huge time-saver for balancing difficulty.

- **Optimized Object Pooling:** 
Since the game involves many customers entering and leaving the store, I implemented an **Object Pooling** system. Instead of constantly creating and destroying NPC objects (which causes lag), I recycle them. I also used `CancellationTokenSource` to ensure that any background tasks are safely stopped when an object is returned to the pool.

---

###  Current Systems Implemented
- **Scan System:** A "Focus-based" scanning mechanic where players must hover over barcodes and click at the right rhythm.
- **Queue Manager:** A FIFO (First-In-First-Out) system that handles customer navigation using NavMesh and manages positions at the counter.
- **Microwave Mechanic:** A system involving vague customer instructions and a "Stress Slider" that responds to the player's current pressure level.
- **Phase Controller:** Manages the transition between different levels of "Rush Hour" intensity over the course of the game session.

---

###  Tech Stack
*   **Engine:** Unity (URP)
*   **Language:** C#
*   **Async Library:** UniTask
*   **Camera:** Cinemachine
*   **Input:** Unity New Input System

---

**Note:** This project is currently a **Work in Progress**. New systems like the "Yeet" (Trash disposal) and "Cram" (Shelf refilling) are being integrated.
