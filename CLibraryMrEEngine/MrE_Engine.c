#include <stdio.h>
#include <stdlib.h>
#include <time.h>
#include <math.h>
#include <R.h>
#include <Rdefines.h>
#include <Rinternals.h>
#include <Rmath.h>

#define MAXNUMBEROFCONSTRAINTS 100
#define MAXNUMBEROFVARIABLES 100
#define MAXNUMBEROFSIGMAPOINTS 1000000
#define MAX_ITERATIONS_LAGRANGEMULTIPLIERS 500

long m_variables_count = 0;
long m_constraints_count = 0;
long m_sigma_points_count = 0;

typedef struct CustomVariable
{
   double MinValue;
   double MaxValue;
} CustomVariable;

typedef struct CustomConstraint
{
   long SparsityNegativeVersusPositive;
   SEXP FunctionPointer;
   SEXP EnvironmentPointer;
   double Values_at_sigma_points[MAXNUMBEROFSIGMAPOINTS];
   double LagrangeMultiplierValue;
   int IsValid; // 0 - invalid, other - valid
} CustomConstraint;

typedef struct SigmaPoint
{
	double DensityValue;
	double VectorCoordinates[MAXNUMBEROFVARIABLES];
	int IsValid; // 0 when invalid; 1 when sampling constraint is satisfied
} SigmaPoint;

CustomVariable* m_variables;
CustomConstraint* m_constraints;
SigmaPoint* m_sigma_points;

long m_LastIterationOfLagrangeMultipliersShifts = 0;
int m_ExpEvidenceScale = 0;
double m_PrecisionOfLagrangeMultiplierAtZeroValue = 0.000001;
double GOLDENRATIO = 1.6180339887;

static SEXP m_distribution_fun, m_distribution_fun_env, m_sampling_constraint_fun;

static SEXP constraint_fun, constraint_env;
static SEXP constraint_fun_internal;
double get_constraint_value(double(*)(),double);

SEXP ConstructMrEEngine(void)
{
	long i = 0;
	long j = 0;
	//test_counter = malloc(sizeof(int));
	//*test_counter = 0;
	m_variables_count = 0;
	m_constraints_count = 0;
	m_sigma_points_count = 0;
	m_variables = malloc(sizeof(CustomVariable) * MAXNUMBEROFVARIABLES);
	m_constraints = malloc(sizeof(CustomConstraint) * MAXNUMBEROFCONSTRAINTS);
	m_sigma_points = malloc(sizeof(SigmaPoint) * MAXNUMBEROFSIGMAPOINTS);
	for(i = 0; i < MAXNUMBEROFVARIABLES;i++)
	{
		CustomVariable var = m_variables[i];
		var.MinValue = -1000;
		var.MaxValue = 1000;
		m_variables[i] = var;
	}
	for(i=0;i<m_constraints_count;i++)
	{
		CustomConstraint cc = m_constraints[i];
		cc.FunctionPointer = R_NilValue;
		cc.EnvironmentPointer = R_NilValue;
		cc.SparsityNegativeVersusPositive = 0;
	    for(j=0;j<MAXNUMBEROFSIGMAPOINTS;j++)
		   cc.Values_at_sigma_points[j] = 0;
	    cc.LagrangeMultiplierValue = 0;
	    cc.IsValid = 1; // 0 - invalid, other - valid
		m_constraints[i] = cc;
	}
	for(i = 0; i < MAXNUMBEROFSIGMAPOINTS;i++)
	{
		SigmaPoint sp = m_sigma_points[i];
		sp.DensityValue = 0;
		sp.IsValid = 0; // By default all are invalid/not populated
		for(j=0;j<MAXNUMBEROFVARIABLES;j++)
		   sp.VectorCoordinates[j] = 0;
		m_sigma_points[i] = sp;
	}
	m_LastIterationOfLagrangeMultipliersShifts = 0;
	m_ExpEvidenceScale = 0;
	return (R_NilValue);
}

SEXP GetVariablesCount(void)
{
	SEXP count;
	PROTECT(count = NEW_INTEGER(1));
	INTEGER(count)[0] = m_variables_count;
	UNPROTECT(1);
	return count;
}

SEXP GetVariableMinValue(SEXP variable_index)
{
	SEXP minValue;
	PROTECT(variable_index = AS_INTEGER(variable_index));
	PROTECT(minValue = NEW_NUMERIC(1));
	REAL(minValue)[0] = m_variables[INTEGER(variable_index)[0]].MinValue;
	UNPROTECT(2);
	return minValue;
}

SEXP GetVariableMaxValue(SEXP variable_index)
{
	SEXP maxValue;
	PROTECT(variable_index = AS_INTEGER(variable_index));
	PROTECT(maxValue = NEW_NUMERIC(1));
	REAL(maxValue)[0] = m_variables[INTEGER(variable_index)[0]].MaxValue;
	UNPROTECT(2);
	return maxValue;
}

SEXP RemoveVariable(SEXP variable_index)
{
	int i = 0;
	PROTECT(variable_index = AS_INTEGER(variable_index));
	int var_index = INTEGER(variable_index)[0];
	if((var_index >= 0) && (var_index < m_variables_count - 1))
	{
		for(i = var_index;i < m_variables_count - 1; i++)
		{
			m_variables[i] = m_variables[i+1];
		}
	}
	m_variables_count--;
	UNPROTECT(1);
	return (R_NilValue);
}

SEXP AddVariableWithRange(SEXP minValue, SEXP maxValue)
{
	PROTECT(minValue = AS_NUMERIC(minValue));
	PROTECT(maxValue = AS_NUMERIC(maxValue));
	CustomVariable var = m_variables[m_variables_count];
	var.MinValue = REAL(minValue)[0];
	var.MaxValue = REAL(maxValue)[0];
	m_variables[m_variables_count] = var;
	m_variables_count++;
	UNPROTECT(2);
	return (R_NilValue);
}

SEXP DisposeMrEEngine(void)
{
	long i = 0;
	for(i=0;i<m_constraints_count;i++)
	{
		CustomConstraint cc = m_constraints[i];
		cc.FunctionPointer = R_NilValue;
		cc.EnvironmentPointer = R_NilValue;
		m_constraints[i] = cc;
	}
	free(m_variables);
	free(m_constraints);
	free(m_sigma_points);
	m_variables_count = 0;
	m_constraints_count = 0;
	m_sigma_points_count = 0;
	return (R_NilValue);
}

SEXP GetConstraintsCount(void)
{
	SEXP count;
	PROTECT(count = NEW_INTEGER(1));
	INTEGER(count)[0] = m_constraints_count;
	UNPROTECT(1);
	return count;
}

SEXP AddNewConstraint(SEXP new_function, SEXP env)
{
	long i = 0;
	CustomConstraint cc;
	cc.SparsityNegativeVersusPositive = 0;
    cc.FunctionPointer = new_function;
	cc.EnvironmentPointer = env;
	for(i=0;i<MAXNUMBEROFSIGMAPOINTS;i++)
		cc.Values_at_sigma_points[i] = 0;
	cc.LagrangeMultiplierValue = 0;
	cc.IsValid = 1; // 0 - invalid, other - valid
	m_constraints[m_constraints_count] = cc;
	m_constraints_count++;
	return (R_NilValue);
}

double get_function_value_at_sigma_point_internal(double (*any_function)(SEXP),double *coordinates)
{
	int i = 0, pc = 0;
	SEXP rlist;
	SEXP coordinate_value;
	
    PROTECT(rlist = NEW_LIST(m_variables_count)); pc++;
	
	for(i=0;i<m_variables_count;i++)
	{
		PROTECT (coordinate_value = NEW_NUMERIC(1));
		REAL(coordinate_value)[0] = coordinates[i];
		SET_ELEMENT(rlist, i, coordinate_value);
		UNPROTECT(1);		
    }    
	double res = any_function(rlist);
	UNPROTECT(pc) ;
	return res;
}

double evalconstraintfun_internal(SEXP rargs)
{
	SEXP Rcall,result;	
	PROTECT(Rcall = lang2(constraint_fun_internal,rargs));
	PROTECT(result = eval(Rcall,constraint_env));
	UNPROTECT(2);
	return(REAL(result)[0]);
}

double evalsamplingconstraintfun_internal(SEXP rargs)
{
	SEXP Rcall,result;	
	PROTECT(Rcall = lang2(m_sampling_constraint_fun,rargs));
	PROTECT(result = eval(Rcall,m_distribution_fun_env));
	UNPROTECT(2);
	return(REAL(result)[0]);
}

double evaldistributionfun_internal(SEXP rargs)
{
	SEXP Rcall,result;	
	PROTECT(Rcall = lang2(m_distribution_fun,rargs));
	PROTECT(result = eval(Rcall,m_distribution_fun_env));
	UNPROTECT(2);
	return(REAL(result)[0]);
}

void TryGenerateUniformSigmaPoints(void)
{
	long k = 0, l = 0, j = 0;
	//time_t t;
	double x;
	// Initialize sigma points first
	for (l = 0; l < m_variables_count; l++)
	{
		CustomVariable var = m_variables[l];
		double diff = var.MaxValue - var.MinValue;
		double max_value = DBL_MIN;
		double min_value = DBL_MAX;
		
		//warning("diff of variable is %f", diff);
		/* Intializes random number generator */
		/* Unfortunately econometric example does not work with this
		 * seed, so had to remove
		 */
		
		//srand((unsigned) time(&t));
		for (k = 0; k < m_sigma_points_count; k++)
		{
			x = ((double)rand()/(double)RAND_MAX); // generate a random number from 0 to 1
			SigmaPoint sp = m_sigma_points[k];
			sp.VectorCoordinates[l] = (x * diff)+var.MinValue;
			if(sp.VectorCoordinates[l] > max_value)
				max_value = sp.VectorCoordinates[l];
			if(sp.VectorCoordinates[l] < min_value)
				min_value = sp.VectorCoordinates[l];
			sp.IsValid = 1; // it became populated, so it became valid, at least temporarily
			m_sigma_points[k] = sp;
		}
		//warning("%d variable max value is %f and min value is %f", l, max_value, min_value);
	}
	// Now we need to set density values and check for validity of sigma points
	double evaldistributionfun_internal(SEXP);
	double evalsamplingconstraintfun_internal(SEXP);
	long new_sigma_points_count = 0;
	long invalid_count = 0;
	for (k = 0; k < m_sigma_points_count; k++)
	{
		SigmaPoint sp = m_sigma_points[k];			
		sp.DensityValue = get_function_value_at_sigma_point_internal(evaldistributionfun_internal, sp.VectorCoordinates);
		if(!isNull(m_sampling_constraint_fun))
		{
			double sampling_constraint_value = get_function_value_at_sigma_point_internal(evalsamplingconstraintfun_internal, sp.VectorCoordinates);
			if(sampling_constraint_value<(double)0)
			{
				sp.IsValid = 0;
				invalid_count++;
			}
			else
				new_sigma_points_count++;
		}
		else
			new_sigma_points_count++;
		m_sigma_points[k] = sp;
	}
	if(invalid_count > 0)
	{
		warning("Sampling constraint rejected %d sigma points out of %d.", invalid_count, m_sigma_points_count);
		// Now "remove" invalid sigma points
	
		long next_good = 1;
		int found_good = 0;
		long double_check = 0;
		for (k = 0; k < m_sigma_points_count - 1; k++)
		{
			SigmaPoint sp = m_sigma_points[k];
			if(sp.IsValid == 1)
			{
				double_check++;
				continue;
			}
			if(next_good <= k)
				next_good = k + 1;
			found_good = 0;
			SigmaPoint next_sp;
			while((found_good == 0) && (next_good < m_sigma_points_count))
			{	
				next_sp = m_sigma_points[next_good];
				if(next_sp.IsValid == 1)
				{
					found_good = 1;
					break;
				}
				next_good++;
			}
			if(found_good == 0)
			{
				warning("Leaving sampling rejection loop at %d", k);
				break;
			}
			// found next valid sigma point	
			for(l=0;l<m_variables_count;l++)
			{
				sp.VectorCoordinates[l] = next_sp.VectorCoordinates[l];
			}
			sp.IsValid = 1;
			sp.DensityValue = next_sp.DensityValue;
			m_sigma_points[k] = sp;
			next_sp.IsValid = 0;
			m_sigma_points[next_good] = next_sp; // mark that next as invalid
			next_good++;
			double_check++;
		}
		if(double_check < new_sigma_points_count - 1)
			warning("A bug in sigma point rejection routine %d and %d.", double_check, new_sigma_points_count);
		
		m_sigma_points_count = new_sigma_points_count;
	}
	for (k = 0; k < MAXNUMBEROFSIGMAPOINTS; k++)
	{
		SigmaPoint sp = m_sigma_points[k];
		if(k < m_sigma_points_count)
		{
			if(sp.IsValid==0)
			{
				warning("There are still invalid sigma point such as the one at %d.", k);
				break;
			}
		}
		else
		{
			if(sp.IsValid==1)
			{
				warning("There are left remaining valid sigma points at %d.", k);
				break;
			}
		}
	}
	
	// Now we need to set constraints values
	double evalconstraintfun_internal(SEXP);
	for (k = 0; k < m_constraints_count; k++)
	{
		CustomConstraint cc = m_constraints[k];			
		
		constraint_fun_internal = cc.FunctionPointer;
		constraint_env = cc.EnvironmentPointer;
		cc.SparsityNegativeVersusPositive = 0;
		cc.LagrangeMultiplierValue = 0;
	    cc.IsValid = 1; // 0 - invalid, other - valid
		j = 0;
		while(j < m_sigma_points_count)
		{
			SigmaPoint sp = m_sigma_points[j];
			x = get_function_value_at_sigma_point_internal(evalconstraintfun_internal, sp.VectorCoordinates);
			cc.Values_at_sigma_points[j] = x;
			if (x < (double)0)
                cc.SparsityNegativeVersusPositive = cc.SparsityNegativeVersusPositive - 1;
            else if(x > (double)0)
                cc.SparsityNegativeVersusPositive = cc.SparsityNegativeVersusPositive + 1;
			j++;
		}
		double sparsity = fabs(m_sigma_points_count - fabs(cc.SparsityNegativeVersusPositive))*100/m_sigma_points_count;
		if(sparsity < 1)
		{
			error("Constraint %d scaling is wrong (Sparsity  coef is %d, sigma count is %d). Consider scaling its variables or the constraint's setpoint", k+1, cc.SparsityNegativeVersusPositive, m_sigma_points_count);
			cc.IsValid = 0;
		}
		else if (sparsity < 10)
		{
			warning("Constraint %d scaling is poor (Sparsity coef is %d, sigma count is %d). Consider scaling its variables or the constraint's setpoint", k+1, cc.SparsityNegativeVersusPositive, m_sigma_points_count);
		}
		m_constraints[k] = cc;
	}
}

SEXP GetLagrangeConstraintsSparsities(void)
{
	long i = 0, pc = 0;
	SEXP rlist;
	SEXP value;
		
    PROTECT(rlist = NEW_LIST(m_constraints_count)); pc++;
	
	for(i=0;i<m_constraints_count;i++)
	{
		PROTECT (value = NEW_INTEGER(1));
		INTEGER(value)[0] = m_constraints[i].SparsityNegativeVersusPositive;
		SET_ELEMENT(rlist, i, value);
		UNPROTECT(1);		
    }    
	UNPROTECT(pc);
	return rlist;
}

SEXP GetLagrangeMultipliers(void)
{
	long i = 0, pc = 0;
	SEXP rlist;
	SEXP value;
		
    PROTECT(rlist = NEW_LIST(m_constraints_count)); pc++;
	
	for(i=0;i<m_constraints_count;i++)
	{
		PROTECT (value = NEW_NUMERIC(1));
		REAL(value)[0] = m_constraints[i].LagrangeMultiplierValue;
		SET_ELEMENT(rlist, i, value);
		UNPROTECT(1);		
    }    
	UNPROTECT(pc);
	return rlist;
}

SEXP GetCoordinatesOfSigmaPoint(SEXP sigma_point_index)
{
	long i = 0, pc = 0;
	SEXP rlist;
	SEXP value;
	PROTECT(sigma_point_index = AS_INTEGER(sigma_point_index)); pc++;
	
	int index = INTEGER(sigma_point_index)[0];
	
    PROTECT(rlist = NEW_LIST(m_variables_count)); pc++;
	
	SigmaPoint sp = m_sigma_points[index - 1];
	for(i=0;i<m_variables_count;i++)
	{
		PROTECT (value = NEW_NUMERIC(1));
		REAL(value)[0] = sp.VectorCoordinates[i];
		SET_ELEMENT(rlist, i, value);
		UNPROTECT(1);		
    }    
	UNPROTECT(pc) ;
	return rlist;
}

SEXP GetDensitiesAtSigmaPoints(SEXP start_sigma_point_index, SEXP end_sigma_point_index)
{
	long i = 0, pc = 0;
	SEXP rlist;
	SEXP value;
	PROTECT(start_sigma_point_index = AS_INTEGER(start_sigma_point_index)); pc++;
	PROTECT(end_sigma_point_index = AS_INTEGER(end_sigma_point_index)); pc++;
	int start = INTEGER(start_sigma_point_index)[0];
	int end = INTEGER(end_sigma_point_index)[0];
	
    PROTECT(rlist = NEW_LIST(end - start)); pc++;
	
	for(i=0;i<(end - start);i++)
	{
		PROTECT (value = NEW_NUMERIC(1));
		REAL(value)[0] = m_sigma_points[i + start - 1].DensityValue;
		SET_ELEMENT(rlist, i, value);
		UNPROTECT(1);		
    }    
	UNPROTECT(pc) ;
	return rlist;
}

SEXP GetConstraintValueAtSigmaPoints(SEXP constraint_index, SEXP start_sigma_point_index, SEXP end_sigma_point_index)
{
	long i = 0, pc = 0;
	SEXP rlist;
	SEXP value;
	PROTECT(start_sigma_point_index = AS_INTEGER(start_sigma_point_index)); pc++;
	PROTECT(end_sigma_point_index = AS_INTEGER(end_sigma_point_index)); pc++;
	PROTECT(constraint_index = AS_INTEGER(constraint_index)); pc++;
	
	int start = INTEGER(start_sigma_point_index)[0];
	int end = INTEGER(end_sigma_point_index)[0];
	int constr_index = INTEGER(constraint_index)[0];
	CustomConstraint cc = m_constraints[constr_index - 1];
	
    PROTECT(rlist = NEW_LIST(end - start)); pc++;
	
	for(i=0;i<(end - start);i++)
	{
		PROTECT (value = NEW_NUMERIC(1));
		REAL(value)[0] = cc.Values_at_sigma_points[i + start - 1];
		SET_ELEMENT(rlist, i, value);
		UNPROTECT(1);		
    }    
	UNPROTECT(pc) ;
	return rlist;
}

SEXP SetDistribution(SEXP distribution_function, SEXP env, SEXP max_sigma_points, SEXP sampling_constraint)
{
	m_distribution_fun = distribution_function;
	m_distribution_fun_env = env;
	m_sampling_constraint_fun = sampling_constraint;
	m_LastIterationOfLagrangeMultipliersShifts = 0;
	m_ExpEvidenceScale = 0;
	
	PROTECT(max_sigma_points = AS_INTEGER(max_sigma_points));
	m_sigma_points_count = INTEGER(max_sigma_points)[0];
	UNPROTECT(1);
	if(m_sigma_points_count > MAXNUMBEROFSIGMAPOINTS)
	{
		warning("Limiting the number of sigma points to %d.", MAXNUMBEROFSIGMAPOINTS);
		m_sigma_points_count = MAXNUMBEROFSIGMAPOINTS;
	}
	
	TryGenerateUniformSigmaPoints();
		
	return (R_NilValue);
}

double GetEvidenceAtLagrangeMultiplierChange(
	double *multipliers, int index_to_check, double value_to_check,
    double *exp_evidence_scale)
{
	exp_evidence_scale[0] = 0;
    double sum_TotalCummulation = 0;
    long indexSigmaPoint = 0;
	long i = 0;
	
	for (indexSigmaPoint = 0; indexSigmaPoint < m_sigma_points_count; indexSigmaPoint++)
    {
		SigmaPoint sigma_point = m_sigma_points[indexSigmaPoint];
		double sum_Beta_mult_Constr = 0;
        for (i = 0; i < m_constraints_count; i++)
        {
            double current_value = multipliers[i];
            if (i == index_to_check)
                current_value = value_to_check;
            sum_Beta_mult_Constr += current_value * m_constraints[i].Values_at_sigma_points[indexSigmaPoint];
        }
		if(indexSigmaPoint==0)
        {
            // here sum_TotalCummulation == 0 and exp_evidence_scale == 0
            if (sum_Beta_mult_Constr < -100)
            {
                double exp_scale = round(sum_Beta_mult_Constr / 100);
                // First time here
                exp_evidence_scale[0] = exp_scale;                        
            }
        }
        sum_TotalCummulation += exp(sum_Beta_mult_Constr - exp_evidence_scale[0] * 100) * sigma_point.DensityValue;
	}
	if (sum_TotalCummulation == 0)
        return 1;
    sum_TotalCummulation /= m_sigma_points_count;	
	return sum_TotalCummulation;
}

void DoPrecisionSamplingOverLagrangeMultipliers(
            double *multipliers, 
            double ratio_for_range,
            double *best_evidence_value,
            int first_time) // 0 false; 1 true
{
	long m = 0, j = 0;
	int maximum_iterations = MAX_ITERATIONS_LAGRANGEMULTIPLIERS;
	double *evidence_scale = malloc(sizeof(double));	
	for (m = 1; m <= maximum_iterations; m++)
    {
        int at_least_one_multiplier_changed = 0; // 0 false; 1 true

        for (j = 0; j < m_constraints_count; j++)
        {
            /* We start iterations
             * we try another value by advancing and 
             * going backward. If nothing wins, then 
             * we leave the current value as is
             * */
			double new_guess = multipliers[j];
            if ((first_time == 1) && (new_guess == 0))
                new_guess = m_PrecisionOfLagrangeMultiplierAtZeroValue;
            else
                new_guess = multipliers[j] * ratio_for_range;
            evidence_scale[0] = 0;
            double sum_TotalCummulation = GetEvidenceAtLagrangeMultiplierChange(multipliers, j, new_guess, evidence_scale);
            if (best_evidence_value[0] > sum_TotalCummulation * exp((evidence_scale[0] - m_ExpEvidenceScale)*100))
            {
                multipliers[j] = new_guess;
                best_evidence_value[0] = sum_TotalCummulation;
                m_ExpEvidenceScale = evidence_scale[0];
                at_least_one_multiplier_changed = 1;
            }
            else
            {
                new_guess = multipliers[j];
                if ((first_time == 1) && (new_guess == 0))
                    new_guess = -m_PrecisionOfLagrangeMultiplierAtZeroValue;
                else
                {
                    new_guess = multipliers[j] / ratio_for_range;
                    if ((first_time == 1) && (fabs(new_guess) < m_PrecisionOfLagrangeMultiplierAtZeroValue))
                        new_guess = 0;   
                }
                sum_TotalCummulation = GetEvidenceAtLagrangeMultiplierChange(multipliers, j, new_guess, evidence_scale);
                if (best_evidence_value[0] > sum_TotalCummulation * exp((evidence_scale[0] - m_ExpEvidenceScale) * 100))
                {
                    multipliers[j] = new_guess;
                    best_evidence_value[0] = sum_TotalCummulation;
                    m_ExpEvidenceScale = evidence_scale[0];
                    at_least_one_multiplier_changed = 1;
                }
            }
        }
        if (!at_least_one_multiplier_changed)
        {
			if (m > m_LastIterationOfLagrangeMultipliersShifts)
                m_LastIterationOfLagrangeMultipliersShifts = m;
            break;
        }
    }
	free(evidence_scale);
}

SEXP Execute(void)
{
	long i = 0;
	double *best_lagrange_multipliers_values = malloc(sizeof(double));
    double *best_evidence_value = malloc(sizeof(double));
	best_evidence_value[0] = DBL_MAX;
	for(i=0;i < m_constraints_count;i++)
	{
		best_lagrange_multipliers_values[i] = 0;
	}
	DoPrecisionSamplingOverLagrangeMultipliers(
        best_lagrange_multipliers_values, 
        GOLDENRATIO, 
        best_evidence_value,
        1);
    DoPrecisionSamplingOverLagrangeMultipliers(
        best_lagrange_multipliers_values,
		1.1,
        best_evidence_value,
        0);
    DoPrecisionSamplingOverLagrangeMultipliers(
        best_lagrange_multipliers_values,
		1.01,
        best_evidence_value,
        0);
	
    for(i=0;i<m_constraints_count;i++)
    {
		CustomConstraint constraint = m_constraints[i];
        constraint.LagrangeMultiplierValue = best_lagrange_multipliers_values[i];
		m_constraints[i] = constraint;
    }
	free(best_evidence_value);
	free(best_lagrange_multipliers_values);		
	return (R_NilValue);
}

double get_constraint_value(double (*constraint_function)(double),double x)
{
	return constraint_function(x);
}

double evalthefun(double x)
{
	SEXP rargs,Rcall,result;
	rargs = allocVector(REALSXP,1);
	REAL(rargs)[0] = x;
	PROTECT(Rcall = lang2(constraint_fun,rargs));
	PROTECT(result = eval(Rcall,constraint_env));
	UNPROTECT(2);
	return(REAL(result)[0]);
}

SEXP do_get_constraint(SEXP args,SEXP function,SEXP env)
{
	double x,res;
	SEXP answer;
	double evalthefun(double);
	constraint_fun = function;
	constraint_env = env;
	x = REAL(args)[0];
	res = get_constraint_value(evalthefun,x);
	PROTECT(answer = allocVector(REALSXP,1));
	REAL(answer)[0] = res;
	UNPROTECT(1);
	return(answer);
}

SEXP set_constraint(SEXP function, SEXP env)
{
	constraint_fun_internal = function;
	constraint_env = env;
	return (R_NilValue);
}

SEXP get_constraint_value_at_sigma_point(SEXP args)
{
	int i = 0;
	double res;
	SEXP answer;
	double evalconstraintfun_internal(SEXP);
	if(!IS_LIST(args))
	{
		error("Arguments must be a list");
		return(R_NilValue);
	}
	if(GET_LENGTH(args) != m_variables_count)
	{
		error("The number of arguments does not match the number of variables");
		return(R_NilValue);
	}
	//x = REAL(args)[0];
	double *x = malloc(sizeof(double) * m_variables_count);	
	for(i = 0; i < m_variables_count; i++)
	{
		x[i] = REAL(args)[i];
	}
	res = get_function_value_at_sigma_point_internal(evalconstraintfun_internal,x);
	free(x);
	PROTECT(answer = allocVector(REALSXP,1));
	REAL(answer)[0] = res;
	UNPROTECT(1);
	return(answer);
}
















































