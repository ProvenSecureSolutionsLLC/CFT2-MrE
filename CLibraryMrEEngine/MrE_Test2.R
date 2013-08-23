

myfunction <- function(x)
{
	return(x * x)
}

myfunction2 <- function(x)
{
	return(as.double(x[1]) * as.double(x[2]) * as.double(x[3]))
}

myfunction2(c(2,3,4))

dyn.load("MrE_Engine.dll")
.Call("ConstructMrEEngine")

.Call("do_get_constraint",c(2),myfunction,new.env())
.Call("do_get_constraint",c(2.2),myfunction,new.env())

minValue= as.double(100)
variable_index = as.integer(1)
.Call("GetVariableMinValue", variable_index, minValue)

minValue = as.double(-1000.1)
maxValue = as.double(1000.1)
.Call("AddVariableWithRange",minValue, maxValue)

 cat("Variables count is ", .Call("GetVariablesCount"),"\n")

minValue = as.double(-100.1)
maxValue = as.double(100.1)
.Call("AddVariableWithRange",minValue, maxValue)

cat("Variables count is ", .Call("GetVariablesCount"),"\n")

minValue = as.double(-1000.3)
maxValue = as.double(1000.3)
.Call("AddVariableWithRange",minValue, maxValue)

cat("Variables count is ", .Call("GetVariablesCount"),"\n")

variable_index = as.integer(2)
cat("MinValue of ", variable_index + 1, " variable is ", .Call("GetVariableMinValue", variable_index), "\n")
cat("MaxValue of ", variable_index + 1, " variable is ", .Call("GetVariableMaxValue", variable_index), "\n")

.Call("set_constraint",myfunction2,new.env())
.Call("get_constraint_value_at_sigma_point",c(2,3,4))

.Call("RemoveVariable", as.integer(1))

variable_index = as.integer(1)
cat("MinValue of ", variable_index + 1, " variable is ", .Call("GetVariableMinValue", variable_index), "\n")
cat("MaxValue of ", variable_index + 1, " variable is ", .Call("GetVariableMaxValue", variable_index), "\n")




.Call("DisposeMrEEngine")
dyn.unload("MrE_Engine.dll")




