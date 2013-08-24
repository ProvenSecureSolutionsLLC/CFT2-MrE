dyn.load("MrE_Engine.dll")
.C("sumSeq", start = as.integer(10), size = as.integer(5), sumVect = as.integer(rep(0, 5)))
.C("fiboSeq", size = as.integer(5), sumVect = as.integer(rep(0, 5)))
.C("sumSeq", start = as.integer(10), size = as.integer(5), sumVect = as.integer(rep(0, 5)))$sumVect
.C("fiboSeq", size = as.integer(5), sumVect = as.integer(rep(0, 5)))$sumVect

