data <- read.csv("wall-results.csv")

# load all required libraries
pacman::p_load(stats, nls2, nlstools, boot, ggplot2, grid, gridExtra, plyr, robustbase, cowplot)

##########################################################################################
# Here are our helper functions 

# multmerge, a function that takes a directory and extension as strings and loads all files in the indicated directory with that extension  
multmerge = function(mypath, ext){
  filenames=list.files(path=mypath, pattern=paste("*\\.", ext, "$", sep=""), full.names=TRUE)
  datalist = lapply(filenames, function(x){read.csv(file=x,header=T)})
  Reduce(function(x,y) {merge(x,y, all=T)}, datalist)
}

# adjusting the ggplot theme
mytheme <-     theme_bw()
# +
#   theme( # remove the vertical grid lines
#     plot.background = element_blank(),
#     legend.position = "none",
#     axis.ticks.margin=unit(c(1.5,-1.0),'mm'),
#     axis.title.y = element_text(size=12),
#     axis.title.x = element_text(size=12, vjust=-0.7),
#     axis.text.y = element_text(size=10, colour="#888888"),
#     axis.text.x = element_text(size=10, colour="#888888"),
#     panel.border = element_rect(fill=NULL, colour="#888888"), # element_blank(),
#     panel.grid.minor.x = element_line(size=0.01, color="#000000"),
#     panel.grid.major.x = element_line(size=0.03, color="#000000"),
#     panel.margin = unit(2, "lines"),
#     axis.line = element_line(size=0.1, color="#000000"),
#     axis.ticks = element_line(size=0.1, color="#000000"),
#     axis.line.x = element_blank(),
#     panel.grid.minor.x = element_line(size=0.01, color="#000000"),
#     panel.grid.major.x = element_line(size=0.03, color="#000000"),
#     strip.background = element_rect(colour="#EEEEEE", fill="#EEEEEE"),
#     strip.text.y = element_text(size=12),
#     strip.text.x = element_text(size=12)
#   )
myaxes <- coord_fixed(ratio = 1, xlim = c(0,100), ylim = c(0,100)) # expand_limits(y=c(0,100), x=c(0,100))


# an inc function for convenience since R doesn't have one
inc <- function(x)
{
  eval.parent(substitute(x <- x + 1))
}

#end of helper functions
#########################################################################################



# data sanity check

# sanity <- c(logical())
# 
# numParticipants  <- length(unique(data$participant))
# okParticipants  <- numParticipants == 10
# sanity  <- append(sanity, okParticipants)
# 
# numConditions <- length(unique(data$condition))
# okConditions  <- numConditions == 4
# sanity  <- append(sanity, okConditions)
# 
# numStimuli  <-  length(unique(data$code))
# okStimuli  <-  numStimuli == 56
# sanity  <- append(sanity, okStimuli)
# 
# numMethods  <-  length(unique(data$method))
# okMethods  <- numMethods == 2
# sanity  <- append(sanity, okMethods)
# 
# numTasks  <- numStimuli * numMethods
# numTasksPerParticipant  <- daply(data, .(participant), nrow)
# okTasks  <- all(numTasksPerParticipant == numTasks)
# sanity  <- append(sanity, okTasks)
# 
# cat(ifelse(all(sanity == TRUE), "Successfully loaded data.\n", paste("There was a problem loading data: \n",
#    "Number of participants:", numParticipants, " -> ", ifelse(okParticipants, "OK", "WRONG") , "\n",
#    "Number of conditions:", numConditions, " -> ", ifelse(okConditions, "OK", "WRONG") , "\n",
#    "Number of stimuli:", numConditions, " -> ", ifelse(okStimuli, "OK", "WRONG") , "\n",
#    "Number of methods:", numMethods, " -> ", ifelse(okMethods, "OK", "WRONG") , "\n",
#    "Number of experimental tasks:", numTasks, " -> ", ifelse(okTasks, "OK", "WRONG") , "\n")))

colnames(data)[which(names(data) == "estimateRatio.Percentage")] <- "estimate"

# create a column indicating whether it's a vertical or a horizontal stimuli

# data <- ddply(data, .(), transform, shape = ifelse(grepl("vertical", condition), "vertical", "horizontal"))
# data  <- ddply(data, .(), transform, method = ifelse(grepl("ratio", method), "ratio estimation", "constant sum"))
# 
# compute the true value from the actual physical size of the stimuli.  

data  <- ddply(data, .(), transform, trueP = size1 / size2 * 100)

# compute the accuracy, i.e., the difference between the estimate and true ratio  

data  <- ddply(data, .(), transform, accuracy = abs(estimate - trueP) )

data  <- ddply(data, .(), transform, rawError = estimate - trueP )


# compute accuracies


# we want bootstrapped confidence intervals, so we need a function to compute them conveniently
# (these convenience functions are borrowed from Pierre Dragicevic)
samplemean <- function(x, d) {
  return(mean(x[d]))
}

# Returns the point estimate and confidence interval in an array of length 3
bootstrapCI <- function(datapoints) {
  datapoints <- datapoints[!is.na(datapoints)]
  # Compute the point estimate
  pointEstimate <- samplemean(datapoints)
  # Make the rest of the code deterministic
  # if (deterministic) set.seed(0)
  # Generate bootstrap replicates
  b <- boot(datapoints, samplemean, R = 10000, parallel="multicore")
  # Compute interval
  ci <- boot.ci(b, type = "bca")
  # Return the point estimate and CI bounds
  # You can print the ci object for more info and debug
  lowerBound <- ci$bca[4]
  upperBound <- ci$bca[5]
  return(c(pointEstimate, lowerBound, upperBound))
}

# a data frame to hold our confidence intervals
accuracies  <- data.frame(pub=character(), orientation=character(), position=numeric(), estimate=numeric(), ci.low=numeric(), ci.high=numeric(), order=integer())

ord <- 6 # adjust this number to the number of values from previous work that are being added further below

# vertical
cis <- bootstrapCI(subset(data, orientation == "vertical" & Position == 2)$accuracy)
accuracies  <- rbind(accuracies, data.frame(pub="", orientation="vertical", position=2, estimate=cis[1], ci.low=cis[2], ci.high=cis[3], order=inc(ord)))

cis <- bootstrapCI(subset(data, orientation == "vertical" & Position == 1)$accuracy)
accuracies  <- rbind(accuracies, data.frame(pub="", orientation="vertical", position=1, estimate=cis[1], ci.low=cis[2], ci.high=cis[3], order=inc(ord)))

cis <- bootstrapCI(subset(data, orientation == "vertical" & Position == 0)$accuracy)
accuracies  <- rbind(accuracies, data.frame(pub="", orientation="vertical", position=0, estimate=cis[1], ci.low=cis[2], ci.high=cis[3], order=inc(ord)))

# horizontal
cis <- bootstrapCI(subset(data, orientation == "horizontal" & Position == 2)$accuracy)
accuracies  <- rbind(accuracies, data.frame(pub="", orientation="horizontal", position = 2, estimate=cis[1], ci.low=cis[2], ci.high=cis[3], order=inc(ord)))

cis <- bootstrapCI(subset(data, orientation == "horizontal" & Position == 1)$accuracy)
accuracies  <- rbind(accuracies, data.frame(pub="", orientation="horizontal", position = 1, estimate=cis[1], ci.low=cis[2], ci.high=cis[3], order=inc(ord)))

cis <- bootstrapCI(subset(data, orientation == "horizontal" & Position == 0)$accuracy)
accuracies  <- rbind(accuracies, data.frame(pub="", orientation="horizontal", position = 0, estimate=cis[1], ci.low=cis[2], ci.high=cis[3], order=inc(ord)))




ord <- 0
# manually entered data from previous work
# TODO here you need to enter data from the work you're replicating so that the reader can compare your results to theirs

prevD  <- data.frame(pub=character(), shape=character(), position=numeric(), estimate=numeric(), ci.low=numeric(), ci.high=numeric(), order=integer())

# prevD  <- rbind(prevD, data.frame(pub="Spence [1990]", shape="line (H)", measure="length", estimate=3.1519, ci.low=2.84, ci.high=3.4479, order=inc(ord)))
# prevD  <- rbind(prevD, data.frame(pub="Spence [1990]", shape="line (V)", measure="length", estimate=3.75, ci.low=3.026, ci.high=4.4479, order=inc(ord)))
# prevD  <- rbind(prevD, data.frame(pub="Spence [1990]", shape="vertical", measure="height", estimate=2.75, ci.low=2.5, ci.high=3.05, order=inc(ord)))
# 
# prevD  <- rbind(prevD, data.frame(pub="Spence [1990]", shape="vertical", measure="height", estimate=3.2434, ci.low=2.8339, ci.high=3.6527, order=inc(ord)))
# prevD  <- rbind(prevD, data.frame(pub="Spence [1990]", shape="cylinder", measure="height", estimate=3.344, ci.low=3.0338, ci.high=3.656, order=inc(ord)))

# merge the two data frames into one for plotting
cidf  <-rbind(prevD, accuracies)


# Plot results
ggplot(data=cidf) +
  mytheme +
  coord_flip() +
  scale_y_continuous(limits = c(0, 13)) +
  # and now we plot the dataframe containing our confidence intervals on top
  geom_pointrange(aes(y=estimate, x=reorder(paste(orientation, position), order), ymin=ci.low, ymax=ci.high), size=0.7) +
  labs(x = "", y = "Average accuracy (absolute discrepancy in percent)") #+
  # annotate("text", x= 3.2, y = 16, label = cidf$pub[1]) +
  # annotate("segment", x=0.5, xend=5.5, y = 12, yend = 12) +
  # annotate("segment", x=5.5, xend=5.5, y = 11.5, yend = 12) +
  # annotate("segment", x=0.5, xend=0.5, y = 11.5, yend = 12)

# save plot; height needs to be adjusted if more data are plotted
ggsave("accuracy.pdf", width = 8, height = 4)



# ------------------------------------------------------


# compute coefficients of psychophysical function (per participant)
coeffs <- ddply(data, .(Position, orientation, participant), function(dat){
  df <- data.frame(x = dat$trueP, y = dat$estimate)
  regSN <- nls(y ~ 100 * (x/100)^a, data = df, start = list(a=1.0), control = list(maxiter = 500))
  coefSN <- coef(regSN)
  a <- coefSN[[1]]

  # compute the residual standard error as a goodness-of-fit metric
  errorfn <- function(reg) sqrt(deviance(reg)/df.residual(reg))
  
  data.frame(participant=dat$participant[1], orientation=dat$orientation[1], Position=dat$Position[1], coef = a)
})

# add the coefficients as a column in our main data file
data <- merge(data, coeffs)




# The following graph illustrates whether there is considerable between participant variation. Each line represents one participant. The closer the individual lines are together, the lower is the variation between participants.

regFun <- function(x, a) {100*(x/100)^a}

# layout graphs in 2 rows, 3 columns
pdf("psychophysical functions per participant.pdf", width = 12, height = 6)
par(mfrow=c(2,3), pty="s")
minA <- 100
maxA <- 0

# horizontal stimuli
## postion 0
curve((x), xlim=c(0,100), ylim=c(0,100), col="red", ylab="predicted estimated ratio (in percent)", xlab="actual ratio (in percent)", main="Horizontal - position 0")
d_ply(subset(data, orientation=="horizontal" & Position== 0), .(participant), function(d){
  curve(regFun(x,a=d$coef[1]), add = TRUE, col=rgb(0,0,0,0.5))
  #p  <- p + geom_smooth(data=d, aes(x = d$trueP, y = d$estimate), method="loess", alpha=I(0.1))
})
text(x=c(20,80), y=c(70, 30), labels = c(prettyNum(min(subset(data, orientation=="horizontal" & Position== 0)$coef), digits=2), prettyNum(max(subset(data, orientation=="horizontal" & Position== 0)$coef), digits=2)))

## position 1
curve((x), xlim=c(0,100), ylim=c(0,100), col="red", ylab="predicted estimated ratio (in percent)", xlab="actual ratio (in percent)", main="Horizontal - position 1")
d_ply(subset(data, orientation=="horizontal" & Position== 1), .(participant), function(d){
  curve(regFun(x,a=d$coef[1]), add = TRUE, col=rgb(0,0,0,0.5))
  #p  <- p + geom_smooth(data=d, aes(x = d$trueP, y = d$estimate), method="loess", alpha=I(0.1))
})
text(x=c(20,80), y=c(70, 30), labels = c(prettyNum(min(subset(data, orientation=="horizontal" & Position== 1)$coef), digits=2), prettyNum(max(subset(data, orientation=="horizontal" & Position== 1)$coef), digits=2)))

## position 2
curve((x), xlim=c(0,100), ylim=c(0,100), col="red", ylab="predicted estimated ratio (in percent)", xlab="actual ratio (in percent)", main="Horizontal - position 2")
d_ply(subset(data, orientation=="horizontal" & Position== 2), .(participant), function(d){
  curve(regFun(x,a=d$coef[1]), add = TRUE, col=rgb(0,0,0,0.5))
  #p  <- p + geom_smooth(data=d, aes(x = d$trueP, y = d$estimate), method="loess", alpha=I(0.1))
})
text(x=c(20,80), y=c(70, 30), labels = c(prettyNum(min(subset(data, orientation=="horizontal" & Position== 2)$coef), digits=2), prettyNum(max(subset(data, orientation=="horizontal" & Position== 2)$coef), digits=2)))

# vertical
## position 0
curve((x), xlim=c(0,100), ylim=c(0,100), col="red", ylab="predicted estimated ratio (in percent)", xlab="actual ratio (in percent)", main="Vertical - position 0")
d_ply(subset(data, orientation=="vertical" & Position== 0), .(participant), function(d){
  curve(regFun(x,a=d$coef[1]), add = TRUE, col=rgb(0,0,0,0.5))
  #p  <- p + geom_smooth(data=d, aes(x = d$trueP, y = d$estimate), method="loess", alpha=I(0.1))
})
text(x=c(20,80), y=c(70, 30), labels = c(prettyNum(min(subset(data, orientation=="vertical" & Position== 0)$coef), digits=2), prettyNum(max(subset(data, orientation=="vertical" & Position== 0)$coef), digits=2)))

## position 1
curve((x), xlim=c(0,100), ylim=c(0,100), col="red", ylab="predicted estimated ratio (in percent)", xlab="actual ratio (in percent)", main="Vertical - position 1")
d_ply(subset(data, orientation=="vertical" & Position== 1), .(participant), function(d){
  curve(regFun(x,a=d$coef[1]), add = TRUE, col=rgb(0,0,0,0.5))
  #p  <- p + geom_smooth(data=d, aes(x = d$trueP, y = d$estimate), method="loess", alpha=I(0.1))
})
text(x=c(20,80), y=c(70, 30), labels = c(prettyNum(min(subset(data, orientation=="vertical" & Position== 1)$coef), digits=2), prettyNum(max(subset(data, orientation=="vertical" & Position== 1)$coef), digits=2)))

## position 2
curve((x), xlim=c(0,100), ylim=c(0,100), col="red", ylab="predicted estimated ratio (in percent)", xlab="actual ratio (in percent)", main="Vertical - position 2")
d_ply(subset(data, orientation=="vertical" & Position== 2), .(participant), function(d){
  curve(regFun(x,a=d$coef[1]), add = TRUE, col=rgb(0,0,0,0.5))
  #p  <- p + geom_smooth(data=d, aes(x = d$trueP, y = d$estimate), method="loess", alpha=I(0.1))
})
text(x=c(20,80), y=c(70, 30), labels = c(prettyNum(min(subset(data, orientation=="vertical" & Position== 2)$coef), digits=2), prettyNum(max(subset(data, orientation=="vertical" & Position== 2)$coef), digits=2)))

dev.off()

