<div align="center">

# Shift25

*A psychological first-person simulation that explores the existential dread of modern labor. Set in a convenience store floating in a void, players enter "Shift 25"—a non-existent hour representing the exhaustion and despair of service work.*

<a href="https://youtu.be/iMtj-iiXoLw?si=oOz16MB4j8ZWaY9C">
  <img src="https://img.youtube.com/vi/iMtj-iiXoLw/maxresdefault.jpg" width="100%">
</a>

<a href="https://youtu.be/iMtj-iiXoLw?si=oOz16MB4j8ZWaY9C">
  <img src="https://img.shields.io/badge/▶%20CLICK%20TO%20WATCH%20DEMO-FF0000?style=for-the-badge&logo=youtube&logoColor=white" />
</a>

**[  Status: Work in Progress / Mini Thesis ]**

</div>

###  Project Overview
**Shift25** is an experimental project built to reflect the burnout and systemic pressure of blue-collar service work. Instead of the clock hitting 00:00, it moves to 25:00. You are trapped in a loop, serving customers, restocking shelves, and managing your stress.

The game uses a **PSX-inspired visual style** with mixed-media elements to create an uncomfortable, surreal atmosphere.

---

###  Architecture
Since this is a **Mini Thesis** project, I focused on building a scalable and professional architecture that can handle complex, overlapping gameplay systems.

- **Asynchronous Task Management (UniTask):** 
I used **UniTask** to handle the game's multi-tasking flow. This allows the microwave to count down and NPCs to move through queues in the background without blocking the main game logic. It makes the game feel responsive while managing many parallel activities.

- **Event-Driven Decoupling (Observer Pattern):** 
To keep the code organized, I built a central `GameEvents` system. When a player scans an item or the "Pressure" increases, the UI and Sound systems react automatically through events. This means systems don't directly call each other—they just notify listeners.

- **Finite State Machine (FSM):** 
NPCs and the Player are governed by a State Pattern. Customers have clear logic for `Browsing`, `Queueing`, and `Scanning`. This prevents bugs like customers walking away while being served and makes adding new behaviors simple.

- **Data-Driven Progression:** 
All balance values—customer spawn rates, item scan times, and the length of each game phase—are stored in **ScriptableObjects**. This allows me to tune the 60-minute gameplay experience entirely in the editor without recompiling.

- **Optimized Object Pooling:** 
Since the game involves many customers entering and leaving the store, I implemented an **Object Pooling** system. Instead of constantly creating and destroying NPC objects (which causes lag), I reuse them efficiently.

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
