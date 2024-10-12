# BreakOverlap

## Algorithm description and illustration
The implementation takes break time inputs and for each break time creates two time markers on a timeline: start time and end time. All the markers are ordered by ascending time value, and if the value is equal, start markes are placed first.
![Alg1](https://github.com/user-attachments/assets/e1686f72-6451-4f7e-8d10-254ed9ace8c5)
From the above exacmple it can be visually determine that the most common break time is from S3-E2. 

After splitting and sorting it is not distinguishable which markers formed which initial input (and it is not important for the algorithm). The algorithm iterates over the timeline and counts open ranges. The number of open ranges at any iteration = drivers on break at that time. Finding the section on the timeline where there are most open ranges (drivers on break = **DoB**) is the most popular break time. Steps are illustrated on the following diagram:
![Alg2](https://github.com/user-attachments/assets/e7aff4a4-3789-4968-b91b-e048c7ad52fc)
The illustration shows that when a "driver's break ends" (=finding an end marker), we have a possible candidate for the most popular break time. Since the number of drivers on break at the end of driver nr 2's break (iteration) was the most it had ever been, the most popular break time became S3-E2. And since all other markers were end markers (meaning no new drivers started a break), the amount of drivers on break never increased and S3-E2 remained the most popular break time. 
