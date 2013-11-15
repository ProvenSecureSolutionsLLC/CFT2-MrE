
SanovTheoremDistribution <- function(x)
{
	var1 = as.double(x[1])
	var2 = as.double(x[2])
	return(
      	(var1 ^ as.integer(11)) *
		(var2 ^ as.integer(2)) *
            ((as.double(1) - var1 - var2) ^ as.integer(7))
	)
}

SanovTheoremSamplingConstraint <- function(x)
{
	# Definition of this constraint is:
	# if it is negative, MrE Engine will refuse to accept freshly sampled sigma point,
	# if positive or zero, the sigma point passes validation
	var1 = as.double(x[1])
	var2 = as.double(x[2])
	return(as.double(1) - var1 - var2)
}

SanovTheoremConstraint <- function(x)
{
	var1 = as.double(x[1])
	var2 = as.double(x[2])
	return(
		(
		(as.double(1) * var1) + 
		(as.double(2) * var2) + 
		(as.double(3) * (as.double(1) - var1 - var2)) - 
		as.double(2.3)
		)
		)
}

SanovTheoremConstraint (c(0.1911204835906995,0.542772234863242))
SanovTheoremDistribution (c(0.1911204835906995,0.542772234863242))

dyn.load("MrE_Engine.dll")
.Call("ConstructMrEEngine")

minValue = as.double(0)
maxValue = as.double(1)
.Call("AddVariableWithRange",minValue, maxValue)
.Call("AddVariableWithRange",minValue, maxValue)

#cat("Variables count is ", .Call("GetVariablesCount"),"\n")
.Call("AddNewConstraint",SanovTheoremConstraint ,new.env())
#cat("Constraints count is ", .Call("GetConstraintsCount"),"\n")
.Call("SetDistribution",SanovTheoremDistribution, new.env(), as.integer(50000), SanovTheoremSamplingConstraint)

.Call("GetLagrangeConstraintsSparsities")

warnings()
#.Call("GetDensitiesAtSigmaPoints",as.integer(1),as.integer(10))
#.Call("GetConstraintValueAtSigmaPoints",as.integer(1), as.integer(1),as.integer(10))


.Call("Execute")
.Call("GetLagrangeMultipliers")
#.Call("GetLagrangeConstraintsSparsities")

.Call("DisposeMrEEngine")
dyn.unload("MrE_Engine.dll")




