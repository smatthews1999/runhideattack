# Run Hide Attack

* A Unity project for NPC (non player characters) AI.

## Getting Started

* Download or clone project, then open in Unity.

### Prerequisites

* This project uses the free edition of [Panda Behavior Trees](http://www.pandabehaviour.com/).  The project will not run without this asset 
installed.
### Description

This AI will direct NPCs to run and hide behind obsticles from the player.  When enough NPCs have gathered together, they will stop running and attack the player.

### Youtube Video
[![IMAGE ALT TEXT HERE](http://img.youtube.com/vi/EjfgQHU-Lpo/0.jpg)](http://www.youtube.com/watch?v=EjfgQHU-Lpo)

* A surface object must contains a Navmesh.  
* Each NPC is a Navmesh Agent.  
* The perimeter vectors from the Navmesh are extracted and  examined as possible cover points for NPCs.
* NPCs that are distant from the Player will be ignored
* NPCs will keep a safe distance from the player.
* The number of NPCs gathered together will determine whether they continue to flee or turn and fight.
* Attacking NPCs will lose interest when they get too far away.
* One attacking, any NPCs in range will join the fight. 

## Built With

* [Unity](https://unity3d.com/) - version 2017.3.1
* [Panda BT](http://www.pandabehaviour.com/) - version 1.4.2

## Authors

* **Sam Matthews** - *Initial work* - [smatthews1999](https://github.com/smatthews1999)

* See also the list of [contributors](https://github.com/smatthews1999/runhideattack/contributors) who participated in this project.

## License

* This project is licensed under the MIT License

## Acknowledgments

* Special thanks to [Stagpoint Developer Tools](http://stagpoint.com/forums/threads/finding-the-borders-of-a-navmesh.10/) for the integral Navmesh functions necessary for building this project.
