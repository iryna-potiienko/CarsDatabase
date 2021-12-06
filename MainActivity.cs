using System;
using System.Collections.Generic;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace CarsDatabase
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        ListView lstViewData;
        List<Car> listSource = new List<Car>();
        Database db;
        private EditText editBrand;
        private EditText editBodyType;
        private EditText editColor;
        private EditText editVEngine;
        private EditText editPrice;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            //FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            //fab.Click += FabOnClick;

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            //Create Database  
            db = new Database();
            db.createDatabase();
            //db.removeTable
            lstViewData = FindViewById<ListView>(Resource.Id.listView);
            editBrand = FindViewById<EditText>(Resource.Id.edtName);
            editBodyType = FindViewById<EditText>(Resource.Id.edtDepart);
            editColor = FindViewById<EditText>(Resource.Id.edtEmail);
            editVEngine = FindViewById<EditText>(Resource.Id.VEngine);
            editPrice = FindViewById<EditText>(Resource.Id.Price);

            var btnAdd = FindViewById<Button>(Resource.Id.btnAdd);
            var btnEdit = FindViewById<Button>(Resource.Id.btnEdit);
            var btnRemove = FindViewById<Button>(Resource.Id.btnRemove);
            var btnAverage = FindViewById<Button>(Resource.Id.btnAverage);
            var btnClear = FindViewById<Button>(Resource.Id.btnClear);
            Button btnQuary = FindViewById<Button>(Resource.Id.btnQuary);
            Button btnAllCars = FindViewById<Button>(Resource.Id.btnAllCars);

            TextView averageVEngine = FindViewById<TextView>(Resource.Id.AverageVEngine);
            //Load Data  
            LoadData();
            //Event  
            btnAdd.Click += delegate
            {
                try
                {
                    Car car = new Car()
                    {
                        Brand = editBrand.Text,
                        BodyType = editBodyType.Text,
                        Color = editColor.Text,
                        VEngine = Convert.ToDouble(editVEngine.Text),
                        Price = Convert.ToDouble(editPrice.Text)
                    };
                    db.insertIntoTable(car);
                    ClearAll();
                    LoadData();
                }catch{}
            };
            btnEdit.Click += delegate
            {
                if (editBrand.Tag != null)
                {
                    Car car = new Car()
                    {
                        Id = int.Parse(editBrand.Tag.ToString()),
                        Brand = editBrand.Text,
                        BodyType = editBodyType.Text,
                        Color = editColor.Text,
                        VEngine = Convert.ToDouble(editVEngine.Text),
                        Price = Convert.ToDouble(editPrice.Text)
                    };
                    db.updateTable(car);
                    ClearAll();
                    LoadData();
                }
            };
            btnRemove.Click += delegate
            {
                if (editBrand.Tag != null)
                {
                    var id = int.Parse(editBrand.Tag.ToString());
                    db.removeTable(id);
                    ClearAll();
                    LoadData();
                }
            };
            lstViewData.ItemClick += (s, e) =>
            {
                //Binding Data  
                var txtBrand = e.View.FindViewById<TextView>(Resource.Id.txtView_Name);
                var txtBodyType = e.View.FindViewById<TextView>(Resource.Id.txtView_Depart);
                var txtColor = e.View.FindViewById<TextView>(Resource.Id.txtView_Email);
                var txtVEngine = e.View.FindViewById<TextView>(Resource.Id.VEngine);
                var txtPrice = e.View.FindViewById<TextView>(Resource.Id.Price);
                editBrand.Text = txtBrand.Text;
                editBrand.Tag = e.Id;
                editBodyType.Text = txtBodyType.Text;
                editColor.Text = txtColor.Text;
                editVEngine.Text = txtVEngine.Text;
                editPrice.Text = txtPrice.Text;
            };
            btnAverage.Click += delegate
            {
                averageVEngine.Text = db.AverageVEngine().ToString();
            };
            btnQuary.Click += delegate
            {
                SelectData();
            };
            btnAllCars.Click += delegate
            {
                LoadData();
            };
            btnClear.Click += delegate
            {
                ClearAll();
            };
        }

        private void LoadData()
        {
            listSource = db.selectTable();
            var adapter = new ListViewAdapter(this, listSource);
            lstViewData.Adapter = adapter;
        }
        private void SelectData()
        {
            listSource = db.SqlQuary();
            var adapter = new ListViewAdapter(this, listSource);
            lstViewData.Adapter = adapter;
        }
        public override void OnBackPressed()
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if(drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            if (id == Resource.Id.nav_contacts)
            {
                var intent = new Intent(this, typeof(ContactsActivity));
                StartActivity(intent);
            }
            else if (id == Resource.Id.nav_map)
            {
                var intent = new Intent(this, typeof(MapsActivity));
                StartActivity(intent);
            }
            else if (id == Resource.Id.nav_aboutMe)
            {
                var intent = new Intent(this, typeof(AboutMe));
                StartActivity(intent);
            }
            
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void ClearAll()
        {
            editBrand.Text = "";
            editColor.Text = "";
            editPrice.Text = "";
            editBodyType.Text = "";
            editVEngine.Text = "";
        }
    }
}

