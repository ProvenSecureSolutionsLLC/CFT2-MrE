# assuming everything is in the same directory and has bben changed to this in R


#load library

library(arm)


#read in data

data = read.table('02linreg.dat')


##begin model string build


mdl <- "
	model {

#Likelihood

	for(i in 1:10){
		y[i] ~ dnorm(mu[i],prec);

mu[i] <- b1*x[i] + b0;


	}

#Priors

	
		b1  ~ dnorm(0,0.000000001);
		b0  ~ dnorm(0,0.1);
		prec <- 1/var;
		var ~ dgamma(10,40);

	

	}
"

	writeLines(mdl,'linreg.bug')

	y <- data[,2]
	x <- data[,1]

	chain1 <- c('x','y')
	parameters <- c('b1','b0','var')

	linreg.sim <- bugs(chain1,inits=NULL,parameters,model.file='linreg.bug',
		n.iter=12000,n.burnin=2000,n.chains=1,n.thin=1,debug=T,codaPkg=T)

	linreg.out <- read.bugs(linreg.sim)
