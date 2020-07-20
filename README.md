# Visual programming of a simulated industrial robot arm in mixed reality using android phone.

The project source files can be found here: https://gitlab.ethz.ch/rcherif/visualprog along with an apk file which can be found under Build/Android and a demo video at the root folder of the project.

### Assets folder:
It contains the following core folders:
- Prefab: contains all the prefab to be displayed such the robot imported urdf
- RosSharp: contains all the library used for RosBridge and urdf import function
- Scenes: contains the single one scene
- Scripts: contain all the scripts
- Urdf: which contains all the information about the robot model description

### Scripts folder:
It contains all the source files and the main files to look at are:
- PlacementController (where all the logic happen)
- KinematicsSolver

The ButtonHandler are used to handle specific action when clickin a button.

The KinematicProblem is a data class used as input for the KinematicsSolver.

The RobotJoint is a data class added as component to each of the robot joint that will need to be moved.

### Implementation details
Use coroutines to mimic a sleep of the activity in order to avoid having multiple touches at the same time due to the update function

Use of a thread dedicated to solve the Kinematic problem to avoid freeze of the application. Used in the path planning case (mode 1), when the user tap the screen to specify the position that should be reached by the robot end-effector.
