﻿using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.IO;

using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Collections.ObjectModel;
///Algorithms Project
///Intelligent Scissors
///

namespace ImageQuantization
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel : IComparable<RGBPixel>
    {
        public byte red, green, blue;
        public RGBPixel(byte r, byte g, byte b)
        {
            this.red = r;
            this.blue = b;
            this.green = g;
        }
        public int CompareTo(RGBPixel that)
        {
            if (this.red == that.red && this.blue == that.blue && this.green == that.green)
                return 0;
            if (this.red > that.red || (this.green > that.green && (this.red == that.red))
                || (this.blue > that.blue && (this.green == that.green) && (this.red == that.red)))
            {
                return 1;
            }
            return -1;
        }
    }



    public struct Vertex
    {
        public double weight;
        public RGBPixel Color;
        public int ClusterID;
        public int ChildsCount;
        public static int BaseClusterID = 1;
        public Vertex(RGBPixel rgb)
        {
            this.weight = 0;
            this.Color = rgb;
            this.ClusterID = 0;
            ChildsCount=0;
        }
        public Vertex(double w, RGBPixel rgb)
        {
            this.weight = w;
            this.Color = rgb;
            this.ClusterID = 0;
            ChildsCount=0;
        }
        public Vertex(double w, RGBPixel rgb, int ClusterID)
        {
            this.weight = w;
            this.Color = rgb;
            this.ClusterID = ClusterID;
            ChildsCount=0;
        }
        public Vertex(double w, RGBPixel rgb, int ClusterID,int ChildsCount)
        {
            this.weight = w;
            this.Color = rgb;
            this.ClusterID = ClusterID;
            this.ChildsCount=ChildsCount;
        }
        public void SetWeight(double Weight)
        {
            this = new Vertex(Weight, this.Color, this.ClusterID,this.ChildsCount);            
        }
        public double GetWeight()
        {
            return weight;
        }
        public void TerminateDistination()
        {
            this = new Vertex(0, this.Color, this.ClusterID,this.ChildsCount);     
        }
        public bool hasDistination()
        {
            return weight == 0 ? false : true;
        }
        public int AssignNew_ClusterID()
        {
            this = new Vertex(this.weight, this.Color, BaseClusterID,this.ChildsCount);            
            BaseClusterID++;
            return ClusterID;
        }
        public void SetChildsCount(int ChildsCount){
            this = new Vertex(weight,Color,ClusterID,ChildsCount);
        }
        public static void ResetBaseID()
        {
            BaseClusterID = 1;
        }
        public bool hasClusterID()
        {
            return ClusterID == 0 ? false : true;
        }
        public int getClusterID()
        {
            return ClusterID;
        }
        public void SetClusterID(int CID)
        {
            this = new Vertex(this.weight, this.Color, CID,this.ChildsCount);          
        }
        public String ToString()
        {
            return this.Color.red + "," + Color.green + "," + Color.blue + "\t  w = " 
                + weight.ToString() + "\t ID = " + ClusterID + "\t ch ="+ChildsCount;
        }
        // to solv the List of Struct Problem
        public static Vertex SetWeight(double weight, Vertex v)
        {
            return new Vertex(weight, v.Color, v.ClusterID,v.ChildsCount);
        }
        public static Vertex SetClusterID(int CID, Vertex v)
        {
            return new Vertex(v.weight, v.Color, CID,v.ChildsCount);
        }
        public static Vertex SetChildsCount(int ChildsCount, Vertex v)
        {
            return new Vertex(v.weight,v.Color,v.ClusterID,ChildsCount);
        }
    }

    public class Cluster
    {
        public double ClusterID;
        public double red, green, blue;
        public double Count;
        private static int BaseID = 1;
        public Cluster()
        {
            ClusterID = BaseID;
            BaseID++;
            red = blue = green = Count = 0;
        }
        public RGBPixel GetAVG()
        {
            byte r, g, b;        
            r = (byte)(Math.Ceiling((double)(red / Count)));
            g = (byte)(Math.Ceiling((double)(green / Count)));
            b = (byte)(Math.Ceiling((double)(blue / Count)));
            return new RGBPixel(r, g, b);
        }
    }

    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        /// 
        public static Vertex[, ,] mst3D;
        public static List<Vertex> distinct;
        public static List<Vertex> distinct2;
        public static System.DateTime t1, t2, t3, t4;                
        public static String Out = "";
        public static String ClusteringState = "";
        public static int Height;
        public static int Width;
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            Height = original_bm.Height;
            Width = original_bm.Width;
            RGBPixel[,] Buffer = new RGBPixel[Height, Width];
            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), 
                    ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb 
                    || original_bm.PixelFormat == PixelFormat.Format32bppRgb
                    || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }


            return Buffer;

        }
        public static RGBPixel[,] Quantize(RGBPixel[,] img, int ClustersCount)
        {
            Out = "";                                                                          //O(1)
            t1 = t2 = t3 = t4 = new System.DateTime();                                         //O(1)
            t1 = DateTime.Now;                                                                 //O(1)
                ConstructMST(img);                                                             //O(D*D)
                t2 = DateTime.Now;                                                             //O(1)
            Out += "Distinct Colors = " + distinct.Count + " Color" + Environment.NewLine;     //O(1)
            Out += "MST min Cost = " + Get_MST_MinCost().ToString() + Environment.NewLine;     //O(1)
            Out += "Constructing MST = " + (t2 - t1).ToString() + Environment.NewLine;         //O(1)

            RGBPixel[,] rImg = new RGBPixel[Height, Width];                                    //O(1)

            t3 = DateTime.Now;                                                                 //O(1)
                InitChildsCount();                                                             //O(D*D)
                Cluster[] Clusters = Cluster(ClustersCount , true);                            //O(K*D)
            t4 = DateTime.Now;                                                                 //O(1)
            Out += "\t Cluster = " + (t4 - t3).ToString() + Environment.NewLine;               //O(1)
            Out += " Total Time             = " + (t4 - t1).ToString() + Environment.NewLine;  //O(1)

            MessageBox.Show(Out);
            if (Clusters != null)
            {
                for (int i = 0; i < Height; i++)                                      //O(H*W)
                {
                    for (int k = 0; k < Width; k++)                                   //O(W)
                    {
                        int r, g, b;                                                  //O(1)
                        r = img[i, k].red;                                            //O(1)
                        g = img[i, k].green;                                          //O(1)
                        b = img[i, k].blue;                                           //O(1)
                        rImg[i, k] = Clusters[mst3D[r, g, b].ClusterID - 1].GetAVG(); //O(1)
                    }
                }
                return rImg;
            }
            return img;
        }


        /// <summary>
        /// Receive : The image as 2d array 
        /// Construct MST and prepar it to The next Step "Clustering"
        /// Return  : Mst3D has the Mts Graph
        /// Complexity :  Î¸(E)   O(D^2) 
        /// </summary>
        public static void ConstructMST(RGBPixel[,] img)
        {
            mst3D = new Vertex[256, 256, 256];                  /// mst3D has The MST 
            distinct = new List<Vertex>();               /// has distinct Colors in The Image 
            Boolean[, ,] distinctColors =
            new Boolean[256, 256, 256]; /// help me to identify the Distinct Colors quickly
            /// Extract The Distinct Colors To distinct List
                ///total :- O( W * H )
                for (int i = 0; i < Height; i++)                                                    //O(H)                                          
                {
                    for (int j = 0; j < Width; j++)                                                 //O(W)
                    {
                        // add this color if doesn't exist 
                        if (!distinctColors[img[i, j].blue, img[i, j].green, img[i, j].red])         //O(1)
                        {
                            distinct.Add(
                                new Vertex(new RGBPixel(img[i,j].red,img[i,j].green,img[i,j].blue)));//O(1)
                            // set this Color true to prevent The duplication 
                            distinctColors[img[i, j].blue, img[i, j].green, img[i, j].red] = true;   //O(1)
                        }
                    }
                }
            int index = 0;
            int currentIndex = 0;
            int Cnt = distinct.Count;
            double minWeight = 0;
                // inti The MTS  ---  the First Color with other Colors 
                //total :- O(D)
                for (int i = 1; i < Cnt; i++)                                                        //O(D)         
                {
                    double current_weight = getWeight(distinct[0].Color, distinct[i].Color);         //O(1)
                    // inti minWeight variable with the frist current_weight     
                    if (i == 1)                                                                      //O(1)   
                    {
                        minWeight = current_weight;                                                  //O(1)
                        index = 1;                                                                   //O(1)
                    }
                    //get min( minWeight,current_weight )
                    else if (current_weight < minWeight)                                             //O(1)
                    {
                        minWeight = current_weight;                                                  //O(1)
                        index = i;                                                                   //O(1)
                    }
                    distinct[i] = new Vertex(current_weight, distinct[i].Color);                     //O(1)
                    // add the edge to Mst 
                    mst3D[distinct[i].Color.red, distinct[i].Color.green, distinct[i].Color.blue] =
                            new Vertex(distinct[i].weight, distinct[0].Color);                          //O(1)
                }
            Cnt--;                                                                               //O(1)
            if (Cnt == index)                                                                    //O(1)
                index = currentIndex;                                                            //O(1)
            // Exclusion  The Current Node 
                swap(Cnt, currentIndex);                                                             //O(1)

            // Execute Prim Algorithm  and Get 
                while (Cnt != 0)                                                          // Î¸(E)   O(D^2) 
                {
                    // set the Current Color
                        currentIndex = index;                                                        // O(1)
                    // loop to update The MST if The Current Edges better 
                    for (int j = 0; j < Cnt; j++){                                                    //O(D)                                       
                            if (currentIndex != j){                                                  // O(1)                    
                                double current_weight = 
                                    getWeight(distinct[currentIndex].Color, distinct[j].Color);     // O(1)
                                // inti minWeight variable to identify the Next node 
                                if (j == 0 || currentIndex == 0 && j == 1){                         // O(1)                         
                                    minWeight = current_weight;                                     // O(1)
                                    index = j;                                                      // O(1)
                                }
                                else if (current_weight < minWeight){  // get minWeight and node index                         
                                    minWeight = current_weight;                                     // O(1)
                                    index = j;                                                      // O(1)
                                }
                                double vertexweigt = distinct[j].weight;                             //O(1)
                                // update if current edge is better than the edge of destination node 
                                if (vertexweigt > current_weight){                        
                                    mst3D[distinct[j].Color.red,
                                            distinct[j].Color.green,
                                            distinct[j].Color.blue] =
                                            new Vertex(current_weight, distinct[currentIndex].Color);   //O(1)
                                    distinct[j] = new Vertex(current_weight, distinct[j].Color);        //O(1)
                                }
                                else{
                                    //if Current edge in MTS has minWeight
                                    //the get minWeight and next node index 
                                    if (minWeight > vertexweigt){                                      //O(1)                            
                                        minWeight = vertexweigt;                                      // O(1)
                                        index = j;                                                    // O(1)
                                    }
                                }                    
                        }
                    }
                    Cnt--;                                                                             //O(1)            
                    if (Cnt == index)            // Swaps the Next Index with CurrentIndex            // O(1)
                        index = currentIndex;                                                         // O(1)
                    swap(Cnt, currentIndex);     // Exclusion  The Current Node                       // O(1)
                }
        }
        
        /// <summary>
        ///  Return :Summation of Weights Of Mst3D
        ///  Complexity : O(1)
        /// </summary>
        public static double Get_MST_MinCost()
        {
            double MinCost = 0;                                                                 //O(1)
            for (int r = 0; r < 256; r++)                                                       //O(1)
                for (int g = 0; g < 256; g++)
                    for (int b = 0; b < 256; b++)
                    {
                        // Count all weights in  MST 
                        MinCost += mst3D[r, g, b].GetWeight();                                  //O(1)
                    }
            return MinCost;                                                                     //O(1)
        }


        /// <summary>
        ///   Received : the image as 2d array 
        ///   Count The Distinct Colors in The Image 
        ///   Return : The Number Of Distinct Colors
        ///   Complexity : O(Height * Width ) 
        /// </summary>
        public static int CountDisincets(RGBPixel[,] img)
        {

            Boolean[, ,] distinctColors = new Boolean[256, 256, 256];

            int Count = 0;                                                                      //O(1)
            for (int i = 0; i < Height; i++)                                         //O(Height*Width)
            {
                for (int j = 0; j < Width; j++)
                {
                    // Count This Color if it didn't Count
                    if (!distinctColors[img[i, j].blue, img[i, j].green, img[i, j].red])
                    {
                        distinctColors[img[i, j].blue, img[i, j].green, img[i, j].red] = true;          //O(1)
                        Count++;                                                                 //O(1)
                    }
                }
            }
            return Count;                                                                        //O(1) 
        }


        /// <summary>
        ///  Construct Clusters[ClustersCount] based on the MST
        /// <para>Complexity :- O(D*K) ,  θ( D*K + 2*D )</para>                
        /// </summary>
        /// <param name="r">the color.Red</param>
        /// <returns>Return Null OR Cluster[]</returns>
        /// 
        // Assignes for Each Color in mst3D a ClusterID,Calculate the AVG Color for each Cluster

        public static Cluster[] Cluster(int ClusersCount,bool SaveToFile)
        {
            //the clusters Count Can NOT be greater than the Colors Count or == 0
            if (ClusersCount <= 0 || ClusersCount >= distinct.Count)
                return null;
            ClusteringState = "";
            //distinct contain all the colors                      
                ////cutting max # weights
                //the cuts = ClusterCount - 1     
                    // total :- O( K * D )
                    for (int i = 0; i < ClusersCount - 1; i++)                         
                    {
                        //find where to Cut
                            double maxWeight = -1;
                            int maxWeightIndex = -1;
                            int maxWeight_ChildsCount = -1;
                            //total :- O( D )
                            for (int k = 0; k < distinct.Count; k++)                     
                            {
                                Vertex lsV = distinct[k];                           //-- O(1)       
                                if (lsV.weight > maxWeight)                         //-- O(1)
                                {
                                    maxWeight = lsV.weight;                         //-- O(1)
                                    maxWeightIndex = k;                             //-- O(1)
                                    maxWeight_ChildsCount = mst3D[lsV.Color.red,    //-- O(1)
                                                            lsV.Color.green, 
                                                            lsV.Color.blue].ChildsCount;
                                }
                                // if they have the same weight SO cut what have more childs
                                else if(lsV.weight == maxWeight){ 
                                    if (mst3D[lsV.Color.red, 
                                        lsV.Color.green,
                                        lsV.Color.blue].ChildsCount > maxWeight_ChildsCount )
                                    {
                                        maxWeight = lsV.weight;                     //-- O(1) 
                                        maxWeightIndex = k;                         //-- O(1) 
                                        maxWeight_ChildsCount = mst3D[lsV.Color.red,//-- O(1) 
                                                                    lsV.Color.green, 
                                                                    lsV.Color.blue].ChildsCount;
                                    }
                   
                                }
                            }
                            distinct[maxWeightIndex] =
                                    Vertex.SetWeight(-1, distinct[maxWeightIndex]);//-- O(1)      
                        // Cut
                            int rr, gg, bb;
                                rr = distinct[maxWeightIndex].Color.red;    //--- O(1)
                                gg = distinct[maxWeightIndex].Color.green;  //--- O(1)
                                bb = distinct[maxWeightIndex].Color.blue;   //--- O(1)
                            //make it has no destination
                                //cuts it's branch from the destination node
                                CutBranch(rr, gg, bb);                      //--- O(D)  
                            mst3D[rr, gg, bb].TerminateDistination();       //--- O(1)                    
                            if (!mst3D[rr, gg, bb].hasClusterID())
                            {
                                mst3D[rr, gg, bb].AssignNew_ClusterID();    //--- O(1)
                            }                     
                    }


                //// Accumelate each Cluster Colors, Accumelate each Cluster AVG
                    Cluster[] Clusters = new Cluster[Math.Max(1, ClusersCount)];
                    for (int i = 0; i < Clusters.GetLength(0); i++)
                        Clusters[i] = new Cluster();

                    //assign the Cluster ID                   
                    // Total :- O( D*D)
                    // Exact :- θ( D )
                    for (int i = 0; i < distinct.Count; i++) 
                    {
                        //Assign
                        int r, g, b;                                       //--- O(1)  
                                r = distinct[i].Color.red;
                                g = distinct[i].Color.green;
                                b = distinct[i].Color.blue;
                                    //calling it D time resulting in run time of ( 2*D )
                            int C_ID = AssignClusterID(r, g, b);            //-- O(D)                                                        
                            distinct[i] = Vertex.SetClusterID(C_ID, distinct[i]);
                        //Accumelate the AVG Color of Each Cluster
                            Clusters[C_ID - 1].red += r;         //--- O(1)
                            Clusters[C_ID - 1].green += g;       //--- O(1)
                            Clusters[C_ID - 1].blue += b;        //--- O(1)
                            Clusters[C_ID - 1].Count++;          //--- O(1)
                    }

                    ////--------------------   checking Clustering
                    //if (SaveToFile)
                    //{
                    //    String f = "";
                    //    f += "distincet colors = " + distinct.Count + Environment.NewLine;
                    //    for (int i = 0; i < Clusters.GetLength(0); i++)
                    //    {
                    //        RGBPixel AVG = Clusters[i].GetAVG();
                    //        f += "Cluster # " + (i + 1).ToString() + '\n' + Environment.NewLine;
                    //        f += "SUM(red) = " + Clusters[i].red + " SUM(green) = " +
                    //        Clusters[i].green + " Sum(blue) = " + Clusters[i].blue + "\n" + Environment.NewLine;
                    //        f += "Count = " + Clusters[i].Count + "\n AVG = " +
                    //            AVG.red + ',' +
                    //            AVG.green + ',' +
                    //            AVG.blue + "\n\n" + Environment.NewLine;
                    //    }
                    //    string path = @"d:\MyTest.txt";
                    //    if (!File.Exists(path))
                    //    {                            
                    //        string createText = "Hello and Welcome" + Environment.NewLine;
                    //        File.WriteAllText(path, createText);
                    //    }
                    //    File.AppendAllText(path, f);
                    //    f = "";
                    //}
            
                ////clear
                Vertex.ResetBaseID();
            
            return Clusters;
        }



        /// <summary>
        /// Assigns a ClusterId to the [r,g,b] color into the mst3D
        /// <para>Complexity :- O(D)</para>        
        /// </summary>
        /// <param name="r">the color.Red</param>
        /// <param name="g">the color.Green</param>
        /// <param name="b">the color.Blue</param>
        /// <returns>the Assigned Cluster ID - starts from 1..</returns>
        public static int AssignClusterID(int r, int g, int b)
        {
            if (mst3D[r, g, b].hasClusterID())
            {
                return mst3D[r, g, b].getClusterID();                       //--- O(1)
            }
            else
            {
                if (!mst3D[r, g, b].hasDistination())
                {                    
                    return mst3D[r, g, b].AssignNew_ClusterID();            //--- O(1)
                }
                else
                {
                    RGBPixel distination = mst3D[r, g, b].Color;            //--- O(1)
                    int distClusterID = AssignClusterID(distination.red,   //--- O(D-1)
                                                        distination.green,
                                                        distination.blue);      
                    mst3D[r, g, b] = Vertex.SetClusterID(distClusterID,
                                                        mst3D[r, g, b]);   //--- O( 1 )

                    return distClusterID;
                }
            }
        }


        /// <summary>
        /// set every Vertex.ChildsCount in the MST3D
        /// MUST be called befor the Clustering 
        /// <para>Complexity :- O(D*D)</para>       
        /// </summary>   
        public static void InitChildsCount()
        {
            //initialy make Every Vertex Knows How many Childs it have !
            //-- O(D*D)
            for (int k = 0; k < distinct.Count; k++)
            {
                Vertex lsV = distinct[k];
                Vertex mst3dV = mst3D[lsV.Color.red,                        //-- O(1)
                                lsV.Color.green,
                                lsV.Color.blue];
                if (!mst3dV.hasDistination() && !mst3dV.hasClusterID())     //-- O(1)
                {   // the MST has one Node that Has No Destination 
                    // So it Assign it to a New Cluster ->i.e. the 1st Cluster
                    mst3D[lsV.Color.red,
                        lsV.Color.green,
                        lsV.Color.blue].AssignNew_ClusterID();              //-- O(1)
                }
                InitChildsCount_aux(lsV.Color.red,
                                lsV.Color.green,
                                lsV.Color.blue,true);                       //-- O(D)
            }            
        }
        /// <summary>
        ///    Use InitChildsCount() instead
        ///    DO NOT Call from any where BUT InitChildsCount()
        ///    Passing is_1stNode True in the calls
        ///    <para>Complexity :- O(D)</para>   
        /// </summary>
        /// <param name="r">the color.Red</param>
        /// <param name="g">the color.Green</param>
        /// <param name="b">the color.Blue</param>
        /// <param name="is_1StNode">Boolean is True if the call is Not Recursive</param>
        public static void InitChildsCount_aux(int r, int g, int b, bool is_1stNode)
        {
            if (!mst3D[r, g, b].hasDistination() && !is_1stNode)              //-- O(1)
            {
                mst3D[r, g, b].SetChildsCount(mst3D[r, g, b].ChildsCount + 1);//-- O(1)
                return;
            }
            else
            {
                if (!is_1stNode)                                              //-- O(1)
                {
                    mst3D[r, g, b].SetChildsCount(mst3D[r, g, b].ChildsCount + 1);// O(1)
                }
                InitChildsCount_aux(mst3D[r, g, b].Color.red, mst3D[r, g, b].Color.green,
                                                  mst3D[r, g, b].Color.blue, false);//O(D-1)
            }
        }


        /// <summary>
        /// subtract the Childs Count from all the parents 
        /// <para>Vertex         edgeToCut </para>
        /// <para> (r,g,b)---------> </para>
        /// <para>Complexity :- O(D - ChildesCount)</para>
        /// </summary>
        /// <param name="r">the Edge wanted to be cutten Src red</param>
        /// <param name="g">the Edge wanted to be cutten Src green</param>
        /// <param name="b">the Edge wanted to be cutten Src blue</param>
        public static void CutBranch(int r, int g, int b )
        {
            Vertex BHead = mst3D[r, g, b];
            CutBranch_aux(BHead.Color.red, BHead.Color.green, 
                BHead.Color.blue, BHead.ChildsCount+1 );
        }
        /// <summary>
        ///    Use CutBranch() instead
        ///    DO NOT Call from any where BUT CutBranch()
        ///    Complexity :- O(D - ChildesCount)
        /// </summary>
        /// <param name="p_r">the Edge wanted to be cutten Dist red</param>
        /// <param name="p_g">the Edge wanted to be cutten Dist green</param>
        /// <param name="p_b">the Edge wanted to be cutten Dist blue</param>
        /// <param name="ChildsCount">the branch to be cutten vertexes count</param>
        public static void CutBranch_aux(int p_r, int p_g, int p_b , int ChildsCount)
        {
            if (!mst3D[p_r, p_g, p_b].hasDistination())                    //--- O(1)
            {
                mst3D[p_r, p_g, p_b].SetChildsCount(
                        mst3D[p_r, p_g, p_b].ChildsCount - ChildsCount
                        );                                                 //--- O(1)
                return;
            }
            else
            {                
                mst3D[p_r, p_g, p_b].SetChildsCount(
                    mst3D[p_r, p_g, p_b].ChildsCount - ChildsCount
                    );                                                      //--- O(1)
                CutBranch_aux(mst3D[p_r, p_g, p_b].Color.red,     //--- O( D - ChidsCount )
                              mst3D[p_r, p_g, p_b].Color.green, 
                              mst3D[p_r, p_g, p_b].Color.blue,
                              ChildsCount);
            }            
        }














































        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }
        private static double getWeight(RGBPixel rgb1, RGBPixel rgb2)
        {
            double rgb = (rgb1.red - rgb2.red) * (rgb1.red - rgb2.red) +
                                   (rgb1.green - rgb2.green) * (rgb1.green - rgb2.green) +
                                   (rgb1.blue - rgb2.blue) * (rgb1.blue - rgb2.blue);
            return Math.Sqrt(rgb);
        }
        public static void swap(int p1, int p2)
        {
            Vertex tmp = distinct[p1];
            distinct[p1] = distinct[p2];
            distinct[p2] = tmp;

        }
        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        public struct RGBPixelD
        {
            public double red, green, blue;
        }
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];


            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }


    }
}






