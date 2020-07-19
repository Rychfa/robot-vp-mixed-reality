# VisualProg

Visual programming of a simulated industrial robot arm in mixed reality using android phone.

The project contains an apk file which can be found under Build/Android and a demo video at the root folder of the project.

The folder Assets contains the following core folders:

- Prefab: contains all the prefab to be displayed such the robot imported urdf
- RosSharp: contains all the library used for RosBridge and urdf import function
- Scenes: contains the single one scene
- Scripts: contain all the scripts
- Urdf: which contains all the information about the robot model description

All the source files are under Scripts and the main files to look at are:
- PlacementController
- KinematicsSolver

The ButtonHandler are used to handle specific action when clickin a button.
The KinematicProblem is a data class used as input for the KinematicsSolver.
The RobotJoint is a data class added as component to each of the robot joint that will need to be moved.
