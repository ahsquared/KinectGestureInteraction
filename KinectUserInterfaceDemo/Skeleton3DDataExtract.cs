//-----------------------------------------------------------------------
// <copyright file="Skeleton2DDataExtract.cs" company="Rhemyst and Rymix">
//     Open Source. Do with this as you will. Include this statement or 
//     don't - whatever you like.
//
//     No warranty or support given. No guarantees this will work or meet
//     your needs. Some elements of this project have been tailored to
//     the authors' needs and therefore don't necessarily follow best
//     practice. Subsequent releases of this project will (probably) not
//     be compatible with different versions, so whatever you do, don't
//     overwrite your implementation with any new releases of this
//     project!
//
//     Enjoy working with Kinect!
// </copyright>
//-----------------------------------------------------------------------

namespace DTWGestureRecognition
{
    using System;
    using System.Windows;
    using System.Windows.Media.Media3D;
    using Microsoft.Research.Kinect.Nui;

    /// <summary>
    /// This class is used to transform the data of the skeleton
    /// </summary>
    internal class Skeleton3DDataExtract
    {
        /// <summary>
        /// Skeleton3DdataCoordEventHandler delegate
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="a">Skeleton 2Ddata Coord Event Args</param>
        public delegate void Skeleton3DdataCoordEventHandler(object sender, Skeleton3DdataCoordEventArgs a);

        /// <summary>
        /// The Skeleton 3Ddata Coord Ready event
        /// </summary>
        public static event Skeleton3DdataCoordEventHandler Skeleton3DdataCoordReady;

        /// <summary>
        /// Crunches Kinect SDK's Skeleton Data and spits out a format more useful for DTW
        /// </summary>
        /// <param name="data">Kinect SDK's Skeleton Data</param>
        public static void ProcessData(SkeletonData data)
        {
            // Extract the coordinates of the points.
            var p = new Point3D[6];
            Point3D shoulderRight = new Point3D(), shoulderLeft = new Point3D();
            foreach (Joint j in data.Joints)
            {
                switch (j.ID)
                {
                    case JointID.HandLeft:
                        p[0] = new Point3D(j.Position.X, j.Position.Y, j.Position.Z);
                        break;
                    case JointID.WristLeft:
                        p[1] = new Point3D(j.Position.X, j.Position.Y, j.Position.Z);
                        break;
                    case JointID.ElbowLeft:
                        p[2] = new Point3D(j.Position.X, j.Position.Y, j.Position.Z);
                        break;
                    case JointID.ElbowRight:
                        p[3] = new Point3D(j.Position.X, j.Position.Y, j.Position.Z);
                        break;
                    case JointID.WristRight:
                        p[4] = new Point3D(j.Position.X, j.Position.Y, j.Position.Z);
                        break;
                    case JointID.HandRight:
                        p[5] = new Point3D(j.Position.X, j.Position.Y, j.Position.Z);
                        break;
                    case JointID.ShoulderLeft:
                        shoulderLeft = new Point3D(j.Position.X, j.Position.Y, j.Position.Z);
                        break;
                    case JointID.ShoulderRight:
                        shoulderRight = new Point3D(j.Position.X, j.Position.Y, j.Position.Z);
                        break;
                }
            }

            // Centre the data
            var center = new Point3D((shoulderLeft.X + shoulderRight.X) / 2, (shoulderLeft.Y + shoulderRight.Y) / 2, (shoulderLeft.Z + shoulderRight.Z) / 2);
            for (int i = 0; i < 6; i++)
            {
                p[i].X -= center.X;
                p[i].Y -= center.Y;
                p[i].Z -= center.Z;
            }

            // Normalization of the coordinates
            double shoulderDist =
                Math.Sqrt(Math.Pow((shoulderLeft.X - shoulderRight.X), 2) +
                          Math.Pow((shoulderLeft.Y - shoulderRight.Y), 2) +
                          Math.Pow((shoulderLeft.Z - shoulderRight.Z), 2));
            for (int i = 0; i < 6; i++)
            {
                p[i].X /= shoulderDist;
                p[i].Y /= shoulderDist;
                p[i].Z /= shoulderDist;
            }

            // Launch the event!
            Skeleton3DdataCoordReady(null, new Skeleton3DdataCoordEventArgs(p));
        }
    }
}