To build and deploy the app do the following
1. Run ExecutorService/VmBuild/build-all.sh<br>
   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; it is advised to run it with arg1 being equal to the absolute path to the compiler microservice.<br>
   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; E.g. /home/janek/Desktop/MicronautCompilerService<br>
   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; The script would ideally be run from working directory ExecutorService/VmBuild.<br>
   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Both of these have fallbacks that will locate the appropriate directory automatically. It will however be time-consuming.<br>
   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; The script should be run as superuser
2. Add the .env file
3. run "sudo docker compose up --build"

***Code Execution will only run on linux as it uses firecracker which relies on linux exclusive KVM***