﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Threading;

//Need rewrite basic auth.

namespace SmartDeviceProject1
{
    public partial class ScanV1 : Form
    {

        //Public variables:
        public string user = ""; //May be better to take when POST-method performed?

        // For goods:
        public Dictionary<string, string> listik = new Dictionary<string, string>();

        // For quick sorting:
        public List<int> intList = new List<int>();
       

        public ScanV1()
        {
            InitializeComponent();
        }

        private void listAdd() 
        {
            // Need write code for new item will be selected in list, for visualize a wheel;

            // For the test:
           // textBox1.Text = "{\"Code\": \"21069974093904\",\"HotMarkingNumber\": \"698135\",\"TaskNumber\": \"21070302\",\"BatchNumber\": \"\",\"MeltingNumber\": \"\",\"Website\": \"https://prommashkomplekt.kz/ru\"}\n";
            //textBox1.Text = textBox1.Text + "{\"Code\": \"21069974093904\",\"HotMarkingNumber\": \"698135\",\"TaskNumber\": \"21070302\",\"BatchNumber\": \"\",\"MeltingNumber\": \"\",\"Website\": \"https://prommashkomplekt.kz/ru\"}\n";

            if (debugMod.Checked) MessageBox.Show(textBox1.Text);

            // Представим алгоритм таким образом:
            // 1) вхождение в строку, ищем символ " как знак открытия ключа
           //2) Считываем символы пока не встретим закрывающую "
           //3) Ищем : как признак значения для ключа
           //4) Находим открывающую " и считываем значение
           //5) после этого если встретим , тогда значит мы пришли к новой итеррации ключ-значение

            if (!textBox1.Text.Contains("HotMarkingNumber") && textBox1.Text != "")
            {

               
                string[] words = textBox1.Text.Split(new char[] { '\r','\n' });

                foreach (string s in words)
                {
                    if (s != "" && !listCodes.Items.Contains(s))
                    {
                        int ss = int.Parse(s);
                        intList.Add(ss);
                        intList.Sort();
                        listCodes.Items.Insert(intList.IndexOf(ss), s);
                    }
                }
                textBox1.Text = "";

                //Можно сделать массив разделителей, и по ним определять коды и в цикле их добавлять в список.
            }

            while (textBox1.Text.Contains("HotMarkingNumber"))
            {
                int end = textBox1.Text.IndexOf("}");
                string block = textBox1.Text.Substring(0, end);
                textBox1.Text = textBox1.Text.Remove(0, end + 1);
                int start = block.IndexOf("\"HotMarkingNumber\": \"") + 21; //hardcode key's lengh
                end = block.IndexOf("\",\"TaskNumber\":");
                string s = block.Substring(start, end - start);
                if (!listCodes.Items.Contains(s))
                {
                    int ss = int.Parse(s);
                    intList.Add(ss);
                    intList.Sort();
                    listCodes.Items.Insert(intList.IndexOf(ss), s);
                    if (debugMod.Checked) MessageBox.Show("Индекс до: " + listCodes.SelectedIndex.ToString());
                    listCodes.SelectedIndex = intList.IndexOf(ss); 
                    if (debugMod.Checked) MessageBox.Show("Индекс после: " + listCodes.SelectedIndex.ToString());

                }
            
            }

            //
            intList.Sort(); // Вопрос сортировки!
            //

            countWheels.Text = "Всего: " + listCodes.Items.Count;
           // listCodes.SelectedIndex = listCodes.Items.Count - 1;
            textBox1.Focus();
        }

        private void loadScreen(bool status)
        {

            if (debugMod.Checked) MessageBox.Show("Статус загрузочного экрана:" + status.ToString());

            if (!status)
            {
                switch (tabMenu.SelectedIndex)
                {
                    case 0:
                        labelAuth.Text = "Выполняется запрос...  Ожидайте";
                        break;
                    default:
                        labelLogin1c.Text = "Выполняется запрос...";
                        labelLogin1c.BackColor = System.Drawing.Color.Blue;
                        break;
                }
            }
            else
            {
                switch (tabMenu.SelectedIndex)
                {
                    case 0:
                        labelAuth.Text = user != "" ? "Авторизация выполнена успешно! " + user : "";
                        break;
                    default:
                        labelLogin1c.Text = labelAuth.Text.Contains("успешно") ? "Авторизован как: " + user : "Авторизация не выполнена!";
                        labelLogin1c.BackColor = labelAuth.Text.Contains("успешно") ? System.Drawing.Color.Green : System.Drawing.Color.Red;
                        break;
                }
                
               
            }

            // Blocking all controls:
            foreach (Control c in this.Controls)
            {
                c.Enabled = status; 
            } 

        }

        //Add code in ListBox:
        private void Button_Click(object sender, EventArgs e) 
        {   
            listAdd();
        }
       
        //Send POST-request:
        private void buttonPost_Click(object sender, EventArgs e)
        {
           //Проверка на лист коды и на заполнение реквизитов.
            if (user == "") 
            { 
                MessageBox.Show("Не выполнена авторизация! Авторизуйтесь на главной странице"); 
                return; 
            }

            if (listCodes.Items.Count == 0) 
            { 
               MessageBox.Show("Список продукции пустой!"); 
               return;
            }


            if (numberDoc.Text == "")
            {
                DialogResult answer1 = MessageBox.Show
                     (
                     "Не указан номер документа! Продолжить?",
                     "Подтверждение",
                     MessageBoxButtons.YesNo,
                     MessageBoxIcon.Question,
                     MessageBoxDefaultButton.Button1
                     );

                if (answer1 == DialogResult.No) return;
            }
            
            DialogResult answer = MessageBox.Show
                (
                "Отправить все данные на сервер?", 
                "Подтверждение", 
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1
                );

            if (answer == DialogResult.Yes)
            {
                string preData = "{\n";  
                //preData += dmKolesa.Text == "" ? "\"dmkolesa\" : \"\",\n" : "\"dmkolesa\" : " + dmKolesa.Text + ",\n";
                //preData += dmStupic.Text == "" ? "\"dmstupic\" : \"\",\n" : "\"dmstupic\" : " + dmStupic.Text + ",\n";
                preData += "\"komment\" : " + "\"" + komment.Text + "\",\n";              
                preData += "\"user\" : " + "\"" + user + "\",\n";
                preData += "\"numberdoc\" : " + "\"" + numberDoc.Text + "\",\n";
                if (nomenclature.Text != "") preData += "\"nomenclature\" : " + "\"" + listik[nomenclature.Text] + "\",\n";
                //Массив кодов:
                preData += "\"masiv\" : [{\n";
                ushort n = 0;

                foreach (string s in listCodes.Items)
                {
                    if(!preData.Contains(s))
                    {
                        preData += "\"thing" + ++n + "\" : \""+ s + "\",\n"; //Этот участок кода следует убрать в процедуру добавления кода.
                    }         
                }
                
                preData = preData.Remove(preData.Length - 2, 2);
                preData += "}]\n}";

                loadScreen(false);
                var url = serverText.Text + nameBase1c.Text + postMethod.Text;

                if (debugMod.Checked)
                    {
                        MessageBox.Show(preData);
                        MessageBox.Show(url.ToString());
                    }

               
                    
                    var request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "POST";
                    request.KeepAlive = false;
                    request.Timeout = 120000; //2 минуты, вдруг вайфай лагать будет

                    //How check the encoding standard?
                    //But if password on russian, and login will be on english? Pair encoding with determining...
                 
                     //var byteArray1 = Encoding.UTF8.GetBytes(authLogin.Text+":");
                     //var byteArray2 = Encoding.ASCII.GetBytes(authPass.Text);
                     //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                   // request.Headers.Add("Authorization", encodedAuth());
                    //Write body:
                    request.Headers.Add("Authorization", "Basic " + "base64 code of login:pass");

                    //UTF8Encoding encoding = new UTF8Encoding(); // - not work!
                    ASCIIEncoding encoding = new ASCIIEncoding();
                    byte[] data = encoding.GetBytes(preData);
                    request.ContentLength = data.Length;
                    Stream newStream = request.GetRequestStream(); //open connection
                    newStream.Write(data, 0, data.Length); // Send the data.
                    newStream.Close();
                    string responseString1 = "";
                try
                {
                    using (HttpWebResponse response1 = (HttpWebResponse)request.GetResponse())
                    {
                       
                        using (var stream1 = response1.GetResponseStream())
                        {
                            using (var reader1 = new StreamReader(stream1))
                            {
                                responseString1 = reader1.ReadToEnd();
                                MessageBox.Show(responseString1);
                            }
                        }
                    }
                }
                catch (Exception ex) 
                    {
                        MessageBox.Show(ex.Message); 
                    }

                if (responseString1.Contains("успешно."))
                {

                    intList.Clear();
                    listCodes.Items.Clear();
                    // Before we clear number, need to write in historydocs:
                    addDocHistory();
                    numberDoc.Text = "";
                    countWheels.Text = "Всего: ";
                    textBox1.Focus();

                }
                else
                {
                    DialogResult answer2 = MessageBox.Show
                          (
                          "Возникли проблемы при создании документа на сервере, очистить список колес?",
                          "Подтверждение",
                          MessageBoxButtons.YesNo,
                          MessageBoxIcon.Question,
                          MessageBoxDefaultButton.Button1
                          );

                    if (answer2 == DialogResult.Yes)
                    {
                        intList.Clear();
                        listCodes.Items.Clear();
                        numberDoc.Text = "";
                        countWheels.Text = "Всего: ";
                        textBox1.Focus();
                    }

                }

                loadScreen(true);
            }// End of condition from question-answer
        }

        //Process of encoding a login and password (not use):
        private string encodedAuth() {

            var byteArray1 = Encoding.ASCII.GetBytes(login.Text + ":"); 
            var byteArray2 = Encoding.ASCII.GetBytes(pass.Text);
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
           return "Basic " + Convert.ToBase64String(byteArray1) + Convert.ToBase64String(byteArray2);
        
        }

        //Close app:
        private void closeButtonClick(object sender, EventArgs e)
        {
            //If current page is the QR, then we ask confirmation:
            if (tabMenu.SelectedIndex == 1)
            {
                DialogResult answer = MessageBox.Show
                    (
                    "Завершить работу программы? Все данные будут очищены",
                    "Подтверждение",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1
                    );

                if (answer == DialogResult.Yes)
                {
                    historyWrite();
                    Application.Exit();
                }
            }
            // end program, clear all variables
            else
            {
                historyWrite();
                Application.Exit();
            }
        }

        //Auntefication 1С:
        private void buttonAuth_Click(object sender, EventArgs e)
        {
            
            labelAuth.Text = ""; 
            //Проверка на заполнение логина и пароля:
            if (login.Text == "") 
                MessageBox.Show("Не заполнен \"Логин\" пользователя для авторизации!");
            else
            {
                var url = serverText.Text + nameBase1c.Text + getMethod.Text + "?Login=" + login.Text + "&Pass=" + pass.Text;

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.KeepAlive = false;
                request.Timeout = 60000;
                //for the test
                //MessageBox.Show(url);
                //string code = encodedAuth();
                //MessageBox.Show(code);
                //
                //request.Headers.Add("Authorization", encodedAuth());
                loadScreen(false);
                request.Headers.Add("Authorization", "Basic " + "base64 code of login:pass");

                if (debugMod.Checked) MessageBox.Show(url.ToString());

                try
                    {
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            string responseString;
                            using (var stream = response.GetResponseStream())
                            {
                                using (var reader = new StreamReader(stream))
                                {
                                    responseString = reader.ReadToEnd();
                                   // MessageBox.Show(responseString);
                                    labelAuth.Text = responseString;
                                }
                            }
                        }
                    }
                     catch (Exception ex) 
                        { 
                            MessageBox.Show(ex.Message); 
                        }

                
                if (labelAuth.Text.Contains("успешно"))
                {
                    user = labelAuth.Text.Remove(0, 31);
                    tabMenu.SelectedIndex = 1;
                    labelLogin1c.Text = "Авторизован как: " + user;
                    labelLogin1c.BackColor = System.Drawing.Color.Green;
                }
                else
                {
                    MessageBox.Show("Неверный логин!");
                }

                loadScreen(true);

            }// end of condition.  
        }

        // Используется при открытии страницы для установки курсора в поле:
        private void tabControl1_GotFocus(object sender, EventArgs e)
        {
            textBox1.Focus();
        }

        // Проверка на пароль, для изменения http сервиса:
        private void checkPass_Click(object sender, EventArgs e)
        {
            // example password:
            if(passSuper.Text == "Danil")
            { 
                serverText.ReadOnly = false;
                nameBase1c.ReadOnly = false;
                getMethod.ReadOnly = false;
                postMethod.ReadOnly = false;
                buttonSaveSet.Enabled = true;
                loadGoods.Enabled = true;
                getGoodsMethod.ReadOnly = false;
                debugMod.Enabled = true;
            }
            else 
            { 
                MessageBox.Show("Пароль не подходит"); 
            }
        }

        // Удаляет элемент из списка кодов:
        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (debugMod.Checked) MessageBox.Show(listCodes.SelectedItem.ToString());

            if (listCodes.SelectedItem != null) 
            {
                intList.Remove(int.Parse(listCodes.SelectedItem.ToString()));
                listCodes.Items.Remove(listCodes.SelectedItem);
                countWheels.Text = "Всего: " + listCodes.Items.Count;
            }
            textBox1.Focus();
        }

        //Системная процедура для скроллера:
        private void ScanV1_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == System.Windows.Forms.Keys.Up))
            {
                // Up
            }
            if ((e.KeyCode == System.Windows.Forms.Keys.Down))
            {
                // Down
            }
            if ((e.KeyCode == System.Windows.Forms.Keys.Left))
            {
                // Left
            }
            if ((e.KeyCode == System.Windows.Forms.Keys.Right))
            {
                // Right
            }
            if ((e.KeyCode == System.Windows.Forms.Keys.Enter))
            {
                // Enter
            }
        }

        //Вызывается для всех полей на странице QR, в которых требуется клавиатура экранная:
        private void sertificate_GotFocus(object sender, EventArgs e)
        { 
           sipPanel.Enabled = true;
        }
        
        // То же что выше, только при потере фокуса - прячет клавиатуру:
        private void sertificate_LostFocus(object sender, EventArgs e)
        {
            sipPanel.Enabled = false;
        }

        //Reading and setting values:
        private void readFile() 
        { 
            
          // This line of code shows current directory:
          // This need for will know about current directory:
          // MessageBox.Show(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase));
            //It is for test. Delete later:
            //string path = "\\Program Files\\config.bin";

            // Open the file to read from:
            string path = "\\Program Files\\nameProgram\\config.bin";
            if (debugMod.Checked) MessageBox.Show(path);
            //Variant for .txt file:
            //using (StreamReader sr = File.OpenText(path))
            //{
            //    string s;
            //    while ((s = sr.ReadLine()) != null)
            //    {
            //        //Заполненение настроек тут добавить надо:
            //        labelAuth.Text = labelAuth.Text + " " + s;
            //    }
            //} 
            if (File.Exists(path))
            {
                using (var stream = File.Open(path, FileMode.Open))
                {
                    using (var reader = new BinaryReader(stream, Encoding.UTF8))
                    {
                        serverText.Text = reader.ReadString();
                        nameBase1c.Text = reader.ReadString();
                        getMethod.Text = reader.ReadString();
                        postMethod.Text = reader.ReadString();
                        //storage.Text = reader.ReadString();
                        getGoodsMethod.Text = reader.ReadString();
                        login.Text = reader.ReadString();
                        pass.Text = reader.ReadString();
                    }
                }
            }else
            {
                MessageBox.Show("Файл с настройками не обнаружен!");
            }

            //Need will add a new parameter, better new file-config with goods codes and name's
            //For now it will be in Russian, without code\article;
            readGoods();
            historyRead();
        }

        private void readGoods()
        {
            if (listik.Count != 0) listik.Clear();
            string path = "\\Program Files\\nameProgram\\goods.bin";
            //test:
            // path = "\\Program Files\\goods.bin";
            if (debugMod.Checked) MessageBox.Show(path);
            if (File.Exists(path))
            {
                using (var stream = File.Open(path, FileMode.Open))
                {
                    using (var reader = new BinaryReader(stream, Encoding.UTF8))
                    {
                        int count = reader.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            string s = reader.ReadString();
                            //MessageBox.Show(s);
                            string a = reader.ReadString();
                            //MessageBox.Show(a);
                            nomenclature.Items.Add(s);
                            //MessageBox.Show("Номенклатуру добавили!");
                            listik.Add(s, a); 
                        }
                    }
                }
            }

        }

        // Write settings (string's) to bin file:
        private void buttonSaveSet_Click(object sender, EventArgs e)
        {
            // Method to write our settings in txt:
            DialogResult answer = MessageBox.Show
                (
                "Вы уверены что хотите перезаписать настройки?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1
                );
            if (answer == DialogResult.Yes)
            {
               

                string path = "\\Program Files\\QRPMK\\config.bin";

                if (debugMod.Checked) MessageBox.Show(path);

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                programFilesPath(); // Check folder!

                using (var stream = File.Open(path, FileMode.OpenOrCreate))
                {
                    using (var writer = new BinaryWriter(stream, Encoding.UTF8))
                    {                       
                        writer.Write(serverText.Text);
                        writer.Write(nameBase1c.Text);
                        writer.Write(getMethod.Text);
                        writer.Write(postMethod.Text);
                        writer.Write(getGoodsMethod.Text);
                        writer.Write(login.Text);
                        writer.Write(pass.Text);
                        
                    }
                }
            }
        }
        
        // System function, needed at startup:
        private void ScanV1_Load(object sender, EventArgs e)
        {
            readFile();   
        }

        // Get request in to base 1C, in order to get list of goods:
        private void loadGoods_Click(object sender, EventArgs e)
        {
            // Суть метода заключается в том чтобы сделать get запрос к базе 1с,
            // для загрузки изменений в номенклатуре базы 1с. Мы получаем ответ с значениями номенклатур, их выводим в список.
            
            var url = serverText.Text + nameBase1c.Text + getGoodsMethod.Text;
            var request = (HttpWebRequest)WebRequest.Create(url);

            if (debugMod.Checked) MessageBox.Show(url.ToString());

            request.Method = "GET";
            request.KeepAlive = false;
            request.Timeout = 60000;
            string responseString ="";
            request.Headers.Add("Authorization", "Basic " + "0JDQtNC80LjQvdC40YHRgtGA0LDRgtC+0YA6engxMjN6eA==");//хардкод
            //request.Headers.Add("Authorization", encodedAuth());
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {

                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            responseString = reader.ReadToEnd();
                        }
                    }
                }
            }     catch (Exception ex) { MessageBox.Show(ex.Message); }

            if (debugMod.Checked) MessageBox.Show("ответ сервера: " + responseString);

            //add pairs:
            if (responseString != "") // I worry about if server back empty answer
            {
                responseString = responseString.TrimEnd(';');
                string[] words = responseString.Split(new char[] { ';' });

                foreach (string s in words)
                {
                    string[] keys = s.Split(new char[] { ':' });
                    listik.Add(keys[0], keys[1]);
                }

                //after server response:
                string path = "\\Program Files\\QRPMK\\goods.bin";
                if (debugMod.Checked) MessageBox.Show(path);
                programFilesPath();
                using (var stream = File.Open(path, FileMode.OpenOrCreate))
                {
                    using (var writer = new BinaryWriter(stream, Encoding.UTF8))
                    {
                        writer.Write(listik.Count);
                        foreach (var pair in listik)
                        {
                            writer.Write(pair.Key);
                            writer.Write(pair.Value);
                        }
                    }
                }
                readGoods();
                MessageBox.Show("Номенклатура обновлена!");
            }
        }

        private void pictureBox2_DoubleClick(object sender, EventArgs e)
        {
            MessageBox.Show("Разработал Котенко Данил (mail: danil8kotenko@gmail.com)");
        }

        // This method cath the key press up, after pressing special button on the scanner:
        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 123 || e.KeyValue == 13)
            {
                listAdd();
            }
        }
 
        //This function is need for debugging and testing.
        private void button6_Click(object sender, EventArgs e)
        {
            programFilesPath();
 
        }
    
        // Saving the list of docs:
        private void historyWrite()
        {

           // if (historyDocs.Items.Count == 0) return;
            
            // string path = "\\Storage Card\\docs.bin";
            string path = "\\Program Files\\nameProgram\\docs.bin";

            if (File.Exists(path))
                {
                    File.Delete(path);
                }

            if (debugMod.Checked) MessageBox.Show(path);
            programFilesPath();
            using (var stream = File.Open(path, FileMode.OpenOrCreate))
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8))
                {

                    foreach (var str in historyDocs.Items)
                    {
                        writer.Write(str.ToString());
                    }
                }
            }
        }

        // Reading list numbers of documents:
        private void historyRead()
        {
            // string path = "\\Storage Card\\docs.bin";
            string path = "\\Program Files\\nameProgram\\docs.bin";
            if (debugMod.Checked) MessageBox.Show(path);
            if (File.Exists(path))
            {
                using (var stream = File.Open(path, FileMode.Open))
                {
                    using (var reader = new BinaryReader(stream, Encoding.UTF8))
                    {
                        string s = "";
                        // Игнорируем ограничение в 10 номеров описи, но это работает только при открытии файла:
                        while (reader.PeekChar() != -1)
                        {
                            s = reader.ReadString();
                            historyDocs.Items.Add(s);
                        }
                    }
                }
            }
        }

        private void addDocHistory()
        {
            // If our list contain smaller than ten elements:
            if (historyDocs.Items.Count < 10) historyDocs.Items.Add("№" + numberDoc.Text + " " + DateTime.Now);
            // However, if zero index contain backup of wheel's, we delete next index:
            else
            {
                for (int i = 0; i <= 9; i++) // Limit on 10 pieces;
                {
                    if (!historyDocs.Items[i].ToString().Contains("бэкап")) //backup
                    {
                        historyDocs.Items.RemoveAt(i);
                        break;
                    }
                }

                historyDocs.Items.Add("№" + numberDoc.Text + " " + DateTime.Now);
            }
        }

        // Save list of codes on the device if response contains "с ошибками" or another fail:
        private void saveCodeList()
        {

            DialogResult answer = MessageBox.Show
                (
                "Сохранить список продукции на устройстве?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button1
                );
            if (answer == DialogResult.Yes)
            {

                // Check that backup docs <= 3:
                int countBackup = 0;
                foreach (var str in historyDocs.Items)
                    if (str.ToString().Contains("бэкап")) countBackup++;

                if (countBackup >= 3)
                {
                    MessageBox.Show("Резервных копий слишком много. Удалите не нужную копию и повторите сохранение.");
                    return;
                }

                // Variant for .txt file:
                string nameFile = "";

                // Delete all symbols except digits:
                foreach (char c in numberDoc.Text)
                    if (char.IsNumber(c)) nameFile += c;


                if (debugMod.Checked) MessageBox.Show(nameFile);

                string path = "\\Program Files\\nameProgram\\" + nameFile + ".txt"; // For the test adress: "Program Files/" Storage Card; 
                if (debugMod.Checked) MessageBox.Show(path);
                //test:
                // path = "\\Program Files\\goods.bin";

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                programFilesPath();

                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {

                    for (ushort i = 0; i < listCodes.Items.Count; i++)
                    {
                        sw.WriteLine(listCodes.Items[i].ToString());
                    }

                }

                // После того как всё успешно сохранили выводим сообщение и очищаем список колес:

                for (int i = 0; i < historyDocs.Items.Count; i++) // Limit on 10 pieces;
                {
                    if (historyDocs.Items[i].ToString().Contains(numberDoc.Text)) //backup
                    {
                        historyDocs.Items.RemoveAt(i);
                        break;
                    }
                }

                historyDocs.Items.Add("№" + numberDoc.Text + " " + DateTime.Now + " бэкап");
                intList.Clear();
                listCodes.Items.Clear();
                numberDoc.Text = "";
                countWheels.Text = "Всего: ";
                textBox1.Focus();
                MessageBox.Show("Документ сохранен!");

            } else historyDocs.Items.Add("№" + numberDoc.Text + " " + DateTime.Now); //answer if;
        }

        private void deleteHistoryDoc(object sender, EventArgs e)
        {
            if (historyDocs.SelectedIndex != -1)
            {
                if (historyDocs.SelectedItem.ToString().Contains("бэкап"))
                {
                    string nameDoc = nameOfDoc(historyDocs.SelectedItem.ToString());
                    string num = "";

                    foreach (char c in nameDoc)
                        if (char.IsNumber(c)) num += c;

                    string path = "\\Program Files\\nameProgram\\" + num + ".txt"; // For the test adress: "Program Files/" Storage Card; 
                    if (debugMod.Checked) MessageBox.Show(path);
                    //test:
                    // path = "\\Program Files\\goods.bin";

                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                
                historyDocs.Items.Remove(historyDocs.SelectedItem);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            historyRead();
        }

        private string nameOfDoc(string preName)
        {
            int end = preName.IndexOf(" ");
            int start = preName.IndexOf("№");
            string nameDoc = preName.Substring(start + 1, end);
            return nameDoc;
        }

        private void downLoadDoc(object sender, EventArgs e)
        {
            // If selected index:
            if (historyDocs.SelectedIndex == -1)
            {
                MessageBox.Show("Не выбран номер документа!");
                return;
            }
            

            numberDoc.Text = nameOfDoc(historyDocs.SelectedItem.ToString());

            

            // Если содержит бэкап тогда:
            if (historyDocs.SelectedItem.ToString().Contains("бэкап"))
            {
                // проверка на то что список колес пуст если нет то предложить очистить.
                // также по другому создается имя документа, так как его мы будем грузить из файла.

                if (listCodes.Items.Count > 0)
                {
                    DialogResult answer2 = MessageBox.Show
                          (
                          "Очистить список продукции?",
                          "Подтверждение",
                          MessageBoxButtons.YesNo,
                          MessageBoxIcon.Question,
                          MessageBoxDefaultButton.Button1
                          );

                        if (answer2 == DialogResult.Yes)
                        {
                            intList.Clear();
                            listCodes.Items.Clear();
                            countWheels.Text = "Всего: ";
                        }
                }
                // Delete all symbols except digits:
               string num = "";
                foreach (char c in numberDoc.Text)
                    if (char.IsNumber(c)) num += c;

                //  Чтение файла и загрузка кодов из него в список кодов:
                string path = "\\Program Files\\nameProgram\\" + num + ".txt"; // For the test adress: "Program Files/" Storage Card; 
                if (debugMod.Checked) MessageBox.Show(path);
                //test:
                // path = "\\Program Files\\goods.bin";

                if (File.Exists(path))
                {
                    using (var stream = File.Open(path, FileMode.Open))
                    {
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            string s = "";
                            while (reader.Peek() != -1)
                            {
                                s = reader.ReadLine();
                                if (!listCodes.Items.Contains(s))
                                {
                                    int ss = int.Parse(s);
                                    intList.Add(ss);
                                    intList.Sort();
                                    listCodes.Items.Insert(intList.IndexOf(ss), s);
                                }
                               
                            }
                        }
                    }
                    countWheels.Text = "Всего: " + listCodes.Items.Count;
                    tabMenu.SelectedIndex = 1;
                    MessageBox.Show("Резервная копия загружена!");
                }

            }
            

        }

        // Form call:
        private void fAddNumber(object sender, EventArgs e)
        {
            if (numberDoc.Text == "")
            {
                tabMenu.SelectedIndex = 1;
                MessageBox.Show("Сперва укажите номер документа!");
                return;
            }

            if (listCodes.Items.Count == 0)
            {
                tabMenu.SelectedIndex = 1;
                MessageBox.Show("Список продукции пуст!");
                return;
            }
         
            saveCodeList();
                
            
        }

        private void programFilesPath()
        {
            string path = "\\Program Files\\nameProgram\\";
            if (!File.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
        }

  
    }// end of class form1;
}