myvar=as.integer(101)
cat(myvar, "\n")
dyn.load("MrE_Engine.dll")
.C("ConstructMrEEngine")
.C("inccounter")
counter = as.integer(1000)
myvar = .C("getcounter", counter)[[1]]
cat(myvar,"\n")
cat("counter result: ", counter, "\n")
.C("inccounter")
.C("inccounter")
.C("inccounter")
myvar = .C("getcounter", counter)[[1]]
cat("counter result: ", myvar, "\n")
.C("DisposeMrEEngine")
dyn.unload("MrE_Engine.dll")

