install.packages(c("Rcpp", "rbenchmark", "inline", "RUnit"))

# do something with Rcpp to quickly check that it works
body <- 'NumericVector xx(x);return wrap( std::accumulate( xx.begin(), xx.end(), 0.0));'
add <- cxxfunction(signature(x = "numeric"), body, plugin = "Rcpp")
 
x <- 1
y <- 2
res <- add(c(x, y))
res