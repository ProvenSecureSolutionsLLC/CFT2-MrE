#include <stdio.h>
#include <R.h>
#include <Rinternals.h>

/*
Filename: "sequence_examples.c"
Return a vectors of sequentially summed values
Arguments:
start -- value to start the sum at
size -- the number of elements to return
sumVect -- the vector of summed output values
*/

void sumSeq(int *start, int *size, int *sumVect){
    /*
    This function provides a simple sequential sum
    where F[n] = F[n-1] + n
    */
    int i, j ;
    j = 0 ;
    for(i = *start; i < (*start + *size); i++){
        if(i == *start){
            sumVect[j] = i ;
        }
        else{
            sumVect[j] = sumVect[j-1] + i ;
        }
        j ++ ;
    }
}

void fiboSeq(int *size, int *sumVect){
    /*
    This function returns the Fibonacci sequence
    where F[n] = F[n-1] + F[n-2]
    */
    int i ;
    sumVect[0] = 0 ;
    sumVect[1] = 1 ;
    for(i = 2; i < *size; i++){
        sumVect[i] = sumVect[i-1] + sumVect[i-2] ;
    }
}

/*
static char *sfunction;
void dosimp(char** funclist, double *start, double *stop,long *n,double *answer)
{
	double sfunc(double);
	double simp(double(*)(),double,double,long);
	sfunction = funclist[0];
	*answer = simp((double(*)())sfunc,*start,*stop,*n);
}

double sfunc(double x)
{
	char *modes[1];
	char *arguments[1];
	double *result;
	long lengths[2];
	lengths[0] = (long)1;
	arguments[0] = (char *)&x;
	modes[0] = "double";
	call_R(sfunction,(long)1,arguments,modes,lengths,(char*)0,(long)1,arguments);
	result = (double*)arguments[0];
	return(*result);
}

double simp(double(*func)(),double start,double stop,long n)
{
	double mult,x,t,inc;
	long i;
	inc = (stop - start) / (double)n;
	t = func(x = start);
	mult = 4.;
	for(i=1;i<n;i++)
	{
		x += inc;
		t += mult*func(x);
		mult = mult == 4. ? 2. : 4.; 
	}
	t += func(stop);
	return(t * inc / 3.);
}
*/








































