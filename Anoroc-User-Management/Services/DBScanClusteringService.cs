﻿using Anoroc_User_Management.Interfaces;
using Anoroc_User_Management.Models;
using DBSCAN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Anoroc_User_Management.Services
{
    public class DBScanClusteringService : IClusterService
    {

        IDatabaseEngine DatabaseService;
        public DBScanClusteringService(IDatabaseEngine database)
        {
            DatabaseService = database;
            //DatabaseService.populate();
        }

        public void AddLocationToCluster(Location location)
        {
            throw new NotImplementedException();
        }

        public List<Cluster> ClustersInRange(Location location, double Distance_To_Cluster_Center)
        {
            // TODO: 
            // var clusterList = DatabaseService.Select_Cluster_In_Area(location.Region);
            // Run through the clusters and check if in range
            // if found, return list of clusters in range    
            // else return null
            return null;
        }

        public List<Location> CheckUnclusteredLocations(Location location, double Direct_Distance_To_Location)
        {

            // TODO: 
            // var locationList = DatabaseService.Select_List_Locations_Unclustered(location.Region);
            // loop through the locationlist and check if any in range
            // return list of locations if one found
            // return null if none found
            return null;
        }



        public List<Cluster> OldClustersInRange(Location location, double Distance_To_Cluster_Center)
        {
            return null;
        }

        public List<Location> CheckOldUnclusteredLocations(Location location, double Direct_Distance_To_Location)
        {
            return null;
        }



        public dynamic GetClusters(Area area)
        {
            var clusters = DatabaseService.Select_List_Clusters();
            return WrapClusters(clusters);
        }
        public dynamic GetClustersPins(Area area)
        {
            var clusters = DatabaseService.Select_List_Clusters();
            return clusters;
        }

        private List<ClusterWrapper> WrapClusters(List<Cluster> clusters)
        {
            List<ClusterWrapper> clusterWrappers = new List<ClusterWrapper>();
            foreach(var cluster in clusters)
            {
                clusterWrappers.Add(new ClusterWrapper(cluster.Coordinates.Count, cluster.Carrier_Data_Points, cluster.Cluster_Radius, cluster.Center_Location));
            }

            return clusterWrappers;
        }

        private List<Cluster> PostProcessClusters(ClusterSet<IPointData> dbscanClusters)
        {
            var clusterWrapper = new List<Cluster>();
            foreach(var clusters in dbscanClusters.Clusters)
            {
                var customCluster = new Cluster();
                for(int i = 0; i < clusters.Objects.Count; i++)
                {
                    PointData pointData = (PointData)clusters.Objects[i];
                    customCluster.AddLocation(new Location(pointData._point.X, pointData._point.Y, pointData.Created, pointData.CarrierDataPoint, pointData.Region));
                }
                customCluster.Structurize();
                clusterWrapper.Add(customCluster);
            }
            return clusterWrapper;
        }

        
        public void GenerateClusters()
        {
            var LocationList = DatabaseService.Select_List_Locations();
           
            IList<IPointData> pointDataList = new List<IPointData>();

            if (LocationList != null)
            {
                LocationList.ForEach(location =>
                {
                    pointDataList.Add(new PointData(location.Latitude, location.Longitude, location.Carrier_Data_Point, location.Created, location.Region));
                });
                var clusters = DBSCAN.DBSCAN.CalculateClusters(pointDataList, epsilon: 0.002, minimumPointsPerCluster: 2);

                var customeClusters = PostProcessClusters(clusters);

                customeClusters.ForEach(cluster =>
                {
                    DatabaseService.Insert_Cluster(cluster);
                });
            }
            else
            {
                // TODO:
                // Error handleing for no locations being recieved
            }
        }
    }
}
