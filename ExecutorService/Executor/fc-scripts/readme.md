### **To provide some context.**
***

#### **~~build-base.sh~~ \[deprecated\]**: <br />
older version of the script that builds our base filesystem image. Changed to build-alp.sh. The decision was largely influenced by slow copies caused by jammy being generally bloated.
<br /><br />
#### **build-alp.sh:** <br />
Will soon replace build-base.sh. Build our base filesystem image fetches some apk packages and prepares an openrc init that gets completed at copy time when it's executed shell script is inserted. Unlike in the case of the older build-base.sh this one completes in ~15s making rebuilding the image often a viable strategy.
<br /><br />
#### **build-copy.sh:** <br />
takes all the parameters necessary for executing pre-compiled java bytecode. The scripts main goal is creating a unique per-execution fully functional alpine linux filesystem. It does so by copying the base image and building on top of it with the actual code to be executed and the script that does the executing.<br /> A future iteration may make an attempt at working with a squash-fs read-only filesystem on top of which another layer is added.<br />
Something to note is that the script works off the presumption that the file containing the to be executed bytecode already exists within the system under /tmp/\<EXEC_ID\> as it will insert said bytecode by directly copying that file. Originally the script itself handled receiving and writing bytecode as one of the call args. The idea was scrapped however the distributed nature of compiling combined with encoding mismatches caused the bytecode to be mangled in transit causing JRE errors.
<br /><br />
##### **java-exec.sh:** <br />
This script makes all the necessary pre-launch configurations and post-execution cleanup. It also attempts to handle vm ttyS0 output using special signing keys to verify the authenticity of control signals sent from within the micro-vm (such as the signal to power down)


