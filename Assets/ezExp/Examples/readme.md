The code should be formatted like so :

Subject, Trial, Color, Shape
1,1,2,"text"
1,2,2,"text"
1,3,1,"text"
...

The output from TouchStone is :

Subject, Trial, "Color", "Shape"
1,1,"Color=2","Shape=text"
1,2,"Color=2","Shape=text"
1,3,"Color=1","Shape=text"
...

- You have to remove the extra " "  in the hrader column
- You have to remove the extra " " in the rows (event when it's strings)
- You have to remove the extra "Column="