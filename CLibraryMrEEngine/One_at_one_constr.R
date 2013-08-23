
OneAtOneDistribution <- function(x)
{
	var1 = as.double(x[1])
	return(dnorm(x=var1, mean = -10, sd = 30))
}

OneAtOneConstraint <- function(x)
{
	var1 = as.double(x[1])
	return (((var1 ^ as.integer(2))-(as.integer(40) * as.integer(40)))/as.integer(100))
}
# a simple test of the constraint function and distribution to make
# sure we did not leave any bugs inside it

OneAtOneDistribution (c(2))
OneAtOneConstraint (c(2))

dyn.load("MrE_Engine.dll")
.Call("ConstructMrEEngine")

.Call("AddVariableWithRange",as.double(-1000), as.double(1000))
.Call("AddNewConstraint",OneAtOneConstraint, new.env())
.Call("SetDistribution",OneAtOneDistribution, new.env(), as.integer(200000), NULL)

#.Call("GetCoordinatesOfSigmaPoint", as.integer(10))
.Call("GetLagrangeConstraintsSparsities")

.Call("Execute")
.Call("GetLagrangeMultipliers")

.Call("DisposeMrEEngine")
dyn.unload("MrE_Engine.dll")




