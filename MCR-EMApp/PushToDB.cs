using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Diagnostics;
namespace ULMSA
{
    class PushToDB                                                   
    {
        public void insertDataToDB(float[] loads)
        {
            try
            {
                //string myConnectionString = ConfigurationManager.AppSettings[0].ToString();
                string myConnectionString = @"Password=um!@#$#;Persist Security Info=True;User ID=unitsMonitor;Initial Catalog=ULM;Data Source=172.21.8.24;pooling=false";
                string newConnectionString=@"Password=rwph;Persist Security Info=True;User ID=rwph;Initial Catalog=RWPH_SCADA;Data Source=172.21.8.24";
                SqlConnection myConnection = new SqlConnection(myConnectionString);
                SqlConnection newConnection = new SqlConnection(newConnectionString);
                if (myConnection.State.ToString() == "Open")
                {
                    myConnection.Close();
                }                
                myConnection.Open();
                newConnection.Open();
                for (int i = 0; i < 9; i++)
                {
                    if (i <7 )
                    {
                        string insertQuery = "insert into unitloads values('unit " + (i + 1).ToString() + "','" + loads[i].ToString() + "','" + DateTime.Now.ToString("G") + "');";
                        SqlCommand commandText = new SqlCommand(insertQuery, myConnection);
                        commandText.CommandTimeout = 200;
                        commandText.ExecuteNonQuery();
                    }
                    else if (i == 7)
                    {
                        string insertQuery = "insert into unitloads values('Frequency " +  "','" + loads[i].ToString() + "','" + DateTime.Now.ToString("G") + "');";
                        SqlCommand commandText = new SqlCommand(insertQuery, myConnection);
                        commandText.ExecuteNonQuery();
                        commandText.CommandTimeout = 200;

                    }
                    else
                    {
                        string insertQuery = "insert into unitloads values('Total "+ "','" + loads[i].ToString() + "','" + DateTime.Now.ToString("G") + "');";
                        SqlCommand commandText = new SqlCommand(insertQuery, myConnection);
                        commandText.CommandTimeout = 200;
                        commandText.ExecuteNonQuery();
                    }
                }
                string countQuery = "select count(*) from dbo.nttps_generation";
                SqlCommand newCommand = new SqlCommand(countQuery, newConnection);
                int rows=(int)newCommand.ExecuteScalar();
                if (rows > 60)
                {
                    string truncateTable = "truncate table dbo.nttps_generation";
                    newCommand = new SqlCommand(truncateTable, newConnection);
                    int success=(int)newCommand.ExecuteNonQuery();                    
                }
//                string query = "insert into dbo.nttps_generation values ('" + DateTime.Now + "','" + loads[0].ToString() + "','" + loads[1].ToString() + "','" + loads[2].ToString() + "','" + loads[3].ToString() + "','" + loads[4].ToString() + "','" + loads[5].ToString() + "','0','" + loads[7].ToString() + "','" + loads[8].ToString() + "')";
                string query = "insert into dbo.nttps_generation values ('"+DateTime.Now + "','" + loads[0].ToString() + "','" + loads[1].ToString() + "','" + loads[2].ToString() + "','" + loads[3].ToString() + "','" + loads[4].ToString() + "','" + loads[5].ToString() + "','" + loads[6].ToString() + "','" + loads[7].ToString() + "','" + loads[8].ToString()+"')";
                newCommand = new SqlCommand(query, newConnection);
                newCommand.ExecuteNonQuery();
                newConnection.Close();
                myConnection.Close();

                //Insert into rwph scada
                string rwphconnectionstring = @"Persist Security Info=False;User ID=rwph;Password=rwph;Initial Catalog=RWPH_SCADA;Data Source=172.21.8.24";
                query = "update PumpHouseCurrentValue set unit1=" + loads[0].ToString() + ", unit2=" + loads[1].ToString() + ", unit3=" + loads[2].ToString() + ", unit4=" + loads[3].ToString() + ", unit5=" + loads[4].ToString() + ", unit6=" + loads[5].ToString() + ", unit7=" + loads[6].ToString() + ", freq=" + loads[7].ToString()+", timestamp='"+DateTime.Now+"'" ;
                newConnection = new SqlConnection(rwphconnectionstring);
                newConnection.Open();
                newCommand = new SqlCommand(query, newConnection);
                newCommand.ExecuteNonQuery();
                newConnection.Close();  
              
                ////Updating tagarchive for sms trips
                //string nttpsconnectionstring = @"server=172.21.8.119;uid=root;pwd=admin;database=nttps";
                //MySqlConnection mysqlConnection = new MySqlConnection(nttpsconnectionstring);
                //for (int i = 1; i < 9; i++)
                //{
                //    query = "update nttps.tagarchive set value=" + loads[i-1].ToString() + " where id="+i;
                //    mysqlConnection.Open();
                //    MySqlCommand mysqlCommand = new MySqlCommand(query, mysqlConnection);
                //    mysqlCommand.ExecuteNonQuery();
                //    mysqlConnection.Close();
                //}



            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("ULMService", "writing data to database an exception has occured" +ex.Message);
            }
        }
        public int updateRunningDays(int rundays,int unit)
        {
            SqlConnection myConnectionString = new SqlConnection(@"Password=um!@#$#;Persist Security Info=True;User ID=unitsMonitor;Initial Catalog=ULM;Data Source=172.21.8.24;pooling=false");
            string countQuery = "update daysrunsofar set runningdays="+rundays+" where Unit='Unit"+unit.ToString()+"'";
            SqlCommand newCommand = new SqlCommand(countQuery, myConnectionString);
            myConnectionString.Open();
            int rows = (int)newCommand.ExecuteNonQuery();
            myConnectionString.Close();
            return rows;
        }
        public object fetchLastSyncDay(int i) 
        {
            SqlConnection myConnectionString = new SqlConnection(@"Password=um!@#$#;Persist Security Info=True;User ID=unitsMonitor;Initial Catalog=ULM;Data Source=172.21.8.24;pooling=false");
            string Query = "select lastsyncdate from daysrunsofar where unit='Unit"+i.ToString()+"'";
            SqlCommand newCommand = new SqlCommand(Query, myConnectionString);
            SqlDataAdapter da = new SqlDataAdapter(newCommand);
            DataSet ds = new DataSet();
            da.Fill(ds);
            return ds.Tables[0].Rows[0][0];
        }


        public void updateLastSyncDate(int i)
        {
            SqlConnection myConnectionString = new SqlConnection(@"Password=um!@#$#;Persist Security Info=True;User ID=unitsMonitor;Initial Catalog=ULM;Data Source=172.21.8.24;pooling=false");
            string countQuery = "update daysrunsofar set lastsyncdate='"+DateTime.Now.Date.ToShortDateString() +"'where Unit='Unit" + i.ToString() + "'";
            SqlCommand newCommand = new SqlCommand(countQuery, myConnectionString);
            myConnectionString.Open();
            newCommand.ExecuteNonQuery();
            myConnectionString.Close();
        }

        public bool isTripped(int p)
        {
            SqlConnection myConnectionString =new SqlConnection(@"Password=um!@#$#;Persist Security Info=True;User ID=unitsMonitor;Initial Catalog=ULM;Data Source=172.21.8.24;pooling=false");
            string Query = "select isTripped from daysrunsofar where unit='Unit" + p.ToString() + "'";
            SqlCommand newCommand = new SqlCommand(Query, myConnectionString);
            SqlDataAdapter da = new SqlDataAdapter(newCommand);
            DataSet ds = new DataSet();
            da.Fill(ds);
            if (int.Parse(ds.Tables[0].Rows[0][0].ToString()) == 1)
                return true;
            else
                return false;

        }

        public void updateTrip(int i,int tripstatus)
        {
            SqlConnection myConnectionString =new SqlConnection(@"Password=um!@#$#;Persist Security Info=True;User ID=unitsMonitor;Initial Catalog=ULM;Data Source=172.21.8.24;pooling=false");
            string countQuery = "update daysrunsofar set istripped="+tripstatus+",runningdays=0 where Unit='Unit" + i.ToString() + "'";
            SqlCommand newCommand = new SqlCommand(countQuery, myConnectionString);
            myConnectionString.Open();
            newCommand.ExecuteNonQuery();
            myConnectionString.Close();
        }

        public void truncateTable()
        {
            SqlConnection myConnectionString = new SqlConnection(@"Password=um!@#$#;Persist Security Info=True;User ID=unitsMonitor;Initial Catalog=ULM;Data Source=172.21.8.24;pooling=false");
            string countQuery = "truncate table unitloads";
            SqlCommand newCommand = new SqlCommand(countQuery, myConnectionString);
            myConnectionString.Open();
            newCommand.ExecuteNonQuery();
            myConnectionString.Close();
        }

        public void fetchLoad(int i)
        { 

        }
    }
}
