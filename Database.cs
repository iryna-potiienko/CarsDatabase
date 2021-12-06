using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarsDatabase
{
    public class Database
    {
        string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        string tableName = "Cars.db";
        public bool createDatabase()
        {
            try
            {
                using (var connection = new SQLiteConnection(System.IO.Path.Combine(folder, tableName)))
                {
                    connection.CreateTable<Car>();
                    return true;
                }
            }
            catch (SQLiteException ex)
            {
                Log.Info("SQLiteEx", ex.Message);
                return false;
            }
        }
        //Add or Insert Operation  

        public bool insertIntoTable(Car person)
        {
            try
            {
                using (var connection = new SQLiteConnection(System.IO.Path.Combine(folder, tableName)))
                {
                    connection.Insert(person);
                    return true;
                }
            }
            catch (SQLiteException ex)
            {
                Log.Info("SQLiteEx", ex.Message);
                return false;
            }
        }
        public List<Car> selectTable()
        {
            try
            {
                using (var connection = new SQLiteConnection(System.IO.Path.Combine(folder, tableName)))
                {
                    var list = connection.Table<Car>().ToList();
                    return list;
                }
            }
            catch (SQLiteException ex)
            {
                Log.Info("SQLiteEx", ex.Message);
                return null;
            }
        }
        //Edit Operation  

        public bool updateTable(Car car)
        {
            try
            {
                using (var connection = new SQLiteConnection(System.IO.Path.Combine(folder, tableName)))
                {
                    connection.Query<Car>("UPDATE Car set Brand=?, BodyType=?, Color=?, VEngine=?, Price=? Where Id=?", car.Brand, car.BodyType, car.Color,car.VEngine, car.Price, car.Id);
                    return true;
                }
            }
            catch (SQLiteException ex)
            {
                Log.Info("SQLiteEx", ex.Message);
                return false;
            }
        }
        //Delete Data Operation  

        public bool removeTable(int Id)
        {
            try
            {
                using (var connection = new SQLiteConnection(System.IO.Path.Combine(folder, tableName)))
                {
                    connection.Query<Car>("DELETE * FROM Car Where Id=?", Id);
                    return true;
                }
            }
            catch (SQLiteException ex)
            {
                Log.Info("SQLiteEx", ex.Message);
                return false;
            }
        }
        //Select Operation  

        public bool selectTable(int Id)
        {
            try
            {
                using (var connection = new SQLiteConnection(System.IO.Path.Combine(folder, tableName)))
                {
                    connection.Query<Car>("SELECT * FROM Car Where Id=?", Id);
                    return true;
                }
            }
            catch (SQLiteException ex)
            {
                Log.Info("SQLiteEx", ex.Message);
                return false;
            }
        }

        public double AverageVEngine()
        {
            List<Car> carsList = new List<Car>();
            double Vsum = 0;

            carsList = selectTable();
            foreach(var car in carsList)
            {
                Vsum += car.VEngine;
            }
            double vAverage = Vsum / carsList.Count();
            return vAverage;
        }
        public List<Car> SqlQuary()
        {
            try
            {
                using (var connection = new SQLiteConnection(System.IO.Path.Combine(folder, tableName)))
                {
                    var list = connection.Query<Car>("SELECT * FROM Car Where Color='Red' AND BodyType='Universal'");
                    return list;
                }
            }
            catch (SQLiteException ex)
            {
                Log.Info("SQLiteEx", ex.Message);
                return null;
            }
        }
    }
}