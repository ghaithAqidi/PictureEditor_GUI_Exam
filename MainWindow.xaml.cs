using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EksamensBesvarelse
{
    /// <summary>
    /// Exam first Assignment picture renamer program.
    /// </summary>
    public partial class MainWindow : Window
    {
        DirectoryInfo dinfo;
        IEnumerable<FileInfo> result = null;
        string currentDir;
        string currentFilePath;
        string desiredNameString;
        string firstImagePath ="";
        bool itemSelected = false;
        string curImage = "";
        DispatcherTimer timer = new DispatcherTimer();
        Clock clock = new Clock();

        public MainWindow()
        {
            InitializeComponent();
            folderBrowserDialogButton.Click += new RoutedEventHandler(folderBrowserDialogButton_Click);
            openFileDialogButton.Click += new RoutedEventHandler(openFileDialogButton_Click);

            //extra: show clock in the statusBar (example of databinding).
            spClock.DataContext = clock;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }

        void openFileDialogButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();        
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //we need to extract name to show on the list without path.
                currentFilePath = dlg.FileName;
                currentDir = System.IO.Path.GetDirectoryName(currentFilePath);
                string currentFile = System.IO.Path.GetFileName(currentFilePath);
                filesNames.Items.Clear();
                if (currentFile != null)
                {
                    filesNames.Items.Add(currentFile);

                    //load the image in memory so that we can rename it without having it locked (IOException)
                    //http://stackoverflow.com/questions/9088443/view-image-file-without-locking-it-copy-to-memory
                    var bitmap = new BitmapImage();
                    var stream = File.OpenRead(currentFilePath);
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    imageContainer.Source = bitmap;
                    stream.Close();
                    stream.Dispose();

                    rename.Visibility = System.Windows.Visibility.Hidden;
                    renameOne.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        //making the browse folder dialog.
        void folderBrowserDialogButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rename.Visibility = System.Windows.Visibility.Visible;
                renameOne.Visibility = System.Windows.Visibility.Hidden;
                currentDir = dlg.SelectedPath;
                dinfo = new DirectoryInfo(currentDir);
                FileInfo[] JPGList = dinfo.GetFiles("*.jpg");
                FileInfo[] JPEGList = dinfo.GetFiles("*.jpeg");
                FileInfo[] PNGList = dinfo.GetFiles("*.png");

                // making a list of picture files of type jpg, jpeg and png.
                result = JPGList.Concat(JPEGList);
                result = result.Concat(PNGList);

                filesNames.Items.Clear();

                if (result != null && result.Any())
                {
                    //showing the first picture in the image element.
                    firstImagePath = currentDir + @"\"+ result.First().ToString();

                    //load the image in memory so that we can rename it without having it locked (IOException)
                    //http://stackoverflow.com/questions/9088443/view-image-file-without-locking-it-copy-to-memory
                    var bitmap = new BitmapImage();
                    var stream = File.OpenRead(firstImagePath);
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    imageContainer.Source = bitmap;
                    stream.Close();
                    stream.Dispose();

                    //adding pictures to the list box:
                    foreach (FileInfo file in result)
                    {
                        filesNames.Items.Add(file.Name);
                    }
                }
                    
                //updating the feedback label (path):
                path.Text = "Files in the path: "+ currentDir + " will be renamed.";
                path.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void FilesNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            itemSelected = true;
            curImage = filesNames.SelectedItem.ToString();
            string currentImagePath = currentDir + @"\" + curImage;

            var bitmap = new BitmapImage();
            var stream = File.OpenRead(currentImagePath);
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            imageContainer.Source = bitmap;
            stream.Close();
            stream.Dispose();


        }

        private void rename_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int counter = 1;
                if (result != null && result.Any())
                {
                    foreach (FileInfo file in result)
                    {
                        //getting the extension of each image:
                        //https://msdn.microsoft.com/en-us/library/system.io.path.getextension(v=vs.110).aspx

                        string fileExtension = System.IO.Path.GetExtension(file.ToString());
                        //https://msdn.microsoft.com/en-us/library/system.io.file.move.aspx
                        if (counter > 999)
                        {
                            //error, the program support naming only 999 files at a time.
                            path.Text = "The program doesnt support naming more than 999 files.";
                            path.Foreground = new SolidColorBrush(Colors.Red);
                            continue;
                        }
                        else if (counter > 99)
                        {
                            //file name doesnt exists in folder, rename and incriment counter.
                            if (!File.Exists(currentDir + @"\" + desiredNameString + " - " + counter + fileExtension))
                            {
                                System.IO.File.Move(currentDir + @"\" + file, currentDir + @"\" + desiredNameString + " - " + counter + fileExtension);
                                counter = counter + 1;

                            }
                            else
                            {
                                // if the file name exists, delete the file and rename to the new name.
                                File.Delete(currentDir + @"\" + desiredNameString + " - 00" + counter + fileExtension);
                                System.IO.File.Move(currentDir + @"\" + file, currentDir + @"\" + desiredNameString + " - " + counter + fileExtension);
                                counter = counter + 1;
                            }
                            continue;
                        }
                        else if (counter > 9)
                        {
                            if (!File.Exists(currentDir + @"\" + desiredNameString + " - 0" + counter + fileExtension))
                            {
                                System.IO.File.Move(currentDir + @"\" + file, currentDir + @"\" + desiredNameString + " - 0" + counter + fileExtension);
                                counter = counter + 1;

                            }
                            else
                            {
                                // if the file name exists, delete the file and rename to the new name.
                                File.Delete(currentDir + @"\" + desiredNameString + " - 00" + counter + fileExtension);
                                System.IO.File.Move(currentDir + @"\" + file, currentDir + @"\" + desiredNameString + " - 0" + counter + fileExtension);
                                counter = counter + 1;
                            }
                            continue;
                        }
                        else
                        {
                            if (!File.Exists(currentDir + @"\" + desiredNameString + " - 00" + counter + fileExtension))
                            {
                                Console.WriteLine(currentDir + @"\" + file);
                                Console.WriteLine(currentDir + @"\" + desiredNameString + " - 00" + counter + fileExtension);
                                System.IO.File.Move(currentDir + @"\" + file, currentDir + @"\" + desiredNameString + " - 00" + counter + fileExtension);
                                counter = counter + 1;

                            }
                            else
                            {
                                // if the file name exists, delete the file and rename to the new name.
                                Console.WriteLine(currentDir + @"\" + desiredNameString + " - 00" + counter + fileExtension);
                                File.Delete(currentDir + @"\" + desiredNameString + " - 00" + counter + fileExtension);
                                System.IO.File.Move(currentDir + @"\" + file, currentDir + @"\" + desiredNameString + " - 00" + counter + fileExtension);
                                counter = counter + 1;
                            }
                        }
                    }
                    //updating label:
                    path.Text = "Files in the path: " + currentDir + " are  renamed.";
                    path.Foreground = new SolidColorBrush(Colors.Green);

                    // get the new names and update the list box:
                    FileInfo[] JPGList = dinfo.GetFiles("*.jpg");
                    FileInfo[] JPEGList = dinfo.GetFiles("*.jpeg");
                    FileInfo[] PNGList = dinfo.GetFiles("*.png");
                    result = JPGList.Concat(JPEGList);
                    result = result.Concat(PNGList);
                    if (!itemSelected)
                    {
                        filesNames.Items.Clear();
                    } else
                    {
                        //when item is selected we cant clear the list.

                    }
                    foreach (FileInfo file in result)
                    {
                        filesNames.Items.Add(file.Name);
                    }
                }
                else
                {
                    // results are empty (directory without pictures or empty)
                    path.Foreground = new SolidColorBrush(Colors.Red);
                    path.Text = "The path: " + currentDir + " is empty or doesnt have any pictures, please select another directory.";
                    string imageNotFoundPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "notFound.jpg");
                    imageContainer.Source = new BitmapImage(new Uri(imageNotFoundPath));

                }
            }
            catch (Exception exception)
            {
                //exception, update the path label with corrosponding feedback.
                path.Text = "Something went wrong: " + exception.ToString();
                path.Foreground = new SolidColorBrush(Colors.Red);
                Console.WriteLine(exception.ToString());

            }
        }
        private void desiredName_TextChanged(object sender, TextChangedEventArgs e)
        {
            desiredNameString = desiredName.Text;
        }

        private void renameOne_Click(object sender, RoutedEventArgs e)
        {
            //rename selected image:
            try
            {
                if (curImage == "")
                {
                    path.Text = "Please select the image from the list. ";
                    path.Foreground = new SolidColorBrush(Colors.Red);
                } else
                {
                    string currentImagePath = currentDir + @"\" + curImage;
                    string fileExtension = System.IO.Path.GetExtension(curImage.ToString());
                    if (!File.Exists(currentDir + @"\" + desiredNameString))
                    {
                        Console.WriteLine("currentImagePath" + currentImagePath);
                        Console.WriteLine("desiredName" + currentDir + @"\" + desiredNameString + fileExtension);
                        System.IO.File.Move(currentImagePath, currentDir + @"\" + desiredNameString + fileExtension);
                        curImage = desiredNameString + fileExtension;
                        Console.WriteLine(currentImagePath);
                    }
                    else
                    {
                        // if the file name exists, show an error message:
                        System.IO.File.Move(currentImagePath, currentDir + @"\" + desiredNameString + fileExtension);

                        path.Text = "This name already exists, please choose another name. ";
                        path.Foreground = new SolidColorBrush(Colors.Red);
                    }

                    //update GUI.
                    dinfo = new DirectoryInfo(currentDir);
                    FileInfo[] JPGList = dinfo.GetFiles("*.jpg");
                    FileInfo[] JPEGList = dinfo.GetFiles("*.jpeg");
                    FileInfo[] PNGList = dinfo.GetFiles("*.png");
                    result = JPGList.Concat(JPEGList);
                    result = result.Concat(PNGList);
                    foreach (FileInfo file in result)
                    {
                        //show only selected file in results:
                        if (file.Name == desiredNameString + fileExtension)
                        {
                            filesNames.Items.Add(file.Name);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                //exception, update the path label with corrosponding feedback.
                path.Text = "Something went wrong: " + exception.ToString();
                path.Foreground = new SolidColorBrush(Colors.Red);
                Console.WriteLine(exception.ToString());
            }
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            clock.Update();
        }

    }
}
