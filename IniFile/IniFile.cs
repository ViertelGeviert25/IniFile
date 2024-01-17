using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace IniFile
{
    /// <summary>
    /// Provides functions for an INI file.
    /// thanks to https://stackoverflow.com/questions/217902/reading-writing-an-ini-file
    /// https://www.codeproject.com/Articles/31597/An-INI-file-enumerator-class-using-C
    /// https://learn.microsoft.com/en-us/windows/win32/api/winbase/
    /// </summary>
    public class IniFile
    {
        private readonly string _filePath;

        /// <summary>
        /// This function writes a value to an (INI) file or deletes certain sections of the (INI) file.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

        /// <summary>
        /// Reads a string value (text) from an (INI) file and returns it as a return value.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="default"></param>
        /// <param name="retVal"></param>
        /// <param name="size"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string section, string key, string @default, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// Reads a string value (text) from an (INI) file and returns it as a return value.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="size"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(string section, int key, string value, [MarshalAs(UnmanagedType.LPArray)] byte[] result, int size, string fileName);

        /// <summary>
        /// Retrieves a string from the specified section in an initialization file.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <param name="size"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [DllImport("kernel32")]
        static extern int GetPrivateProfileString(int section, string key, string value, [MarshalAs(UnmanagedType.LPArray)] byte[] result, int size, string fileName);

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="iniPath"></param>
        public IniFile(string iniPath)
        {
            _filePath = new FileInfo(iniPath).FullName;
        }

        /// <summary>
        /// The Function called to obtain the EntryKey Value from the given SectionHeader and EntryKey string passed, then returned.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public string GetEntryValue(string section, string entry)
        {
            // Sets the maxsize buffer to 250, if the more is required then doubles the size each time. 
            for (int maxsize = 250; true; maxsize *= 2)
            {
                // Obtains the EntryValue information and uses the StringBuilder Function to and stores them in the maxsize buffers (result).
                // Note that the SectionHeader and EntryKey values has been passed.
                StringBuilder result = new StringBuilder(maxsize);
                int size = GetPrivateProfileString(section, entry, string.Empty, result, maxsize, _filePath);
                if (size < maxsize - 1)
                {
                    // Returns the value gathered from the EntryKey
                    return result.ToString();
                }
            }
        }

        /// <summary>
        /// Writes an entry of a section, or overwrites it, if the key already exists.
        /// If the section does not exist, it will be created.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void WriteEntry(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, _filePath);
        }

        /// <summary>
        /// Writes entires to a section, or overwrites them, if the section keys already exist.
        /// If the section does not exist, it will be created.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="entries"></param>
        public void WriteEntries(string section, params Tuple<string, string>[] entries)
        {
            foreach (var entry in entries)
            {
                WriteEntry(section, entry.Item1, entry.Item2);
            }
        }

        /// <summary>
        /// Deletes an entry of a section.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        public void DeleteEntry(string section, string key)
        {
            WriteEntry(section, key, null);
        }

        /// <summary>
        /// Deletes a section.
        /// </summary>
        /// <param name="section"></param>
        public void DeleteSection(string section)
        {
            WriteEntry(section, null, null);
        }

        /// <summary>
        /// Returns true if the specified key exists in the specified section.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns>True if the key exists.</returns>
        public bool KeyExists(string section, string key)
        {
            return GetEntryValue(section, key).Length > 0;
        }

        /// <summary>
        /// Returns true if the specified section exists.
        /// </summary>
        /// <param name="section"></param>
        /// <returns>True if the section exists.</returns>
        public bool SectionExists(string section)
        {
            var sectionNames = GetSectionNames();
            if (sectionNames == null)
            {
                return false;
            }
            return sectionNames.Contains(section);
        }

        /// <summary>
        /// Returns the first section associated with the specified key and value; otherwise null, if not found.
        /// </summary>
        /// <param name="key">The key of the section to get.</param>
        /// <param name="value">The value of the section to get.</param>
        /// <param name="section">The section to find.</param>
        /// <returns>True if the file contains a section with the specified key and value; otherwie, false.</returns>
        public bool TryGetSectionByKeyAndValue(string key, string value, out string section)
        {
            section = null;
            foreach (var loopSection in GetSectionNames().ToList())
            {
                if (GetEntryValue(loopSection, key) == value)
                {
                    section = loopSection;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns all entries of a section.
        /// </summary>
        /// <param name="section"></param>
        /// <returns>Array of tuples.</returns>
        public Tuple<string, string>[] GetEntries(string section)
        {
            var result = new List<Tuple<string, string>>();
            foreach (var key in GetEntryNames(section))
            {
                result.Add(Tuple.Create(key, GetEntryValue(section, key)));
            }
            return result.ToArray();
        }


        /// <summary>
        /// The Function called to obtain the SectionHeaders, and returns them in an Dynamic Array.
        /// </summary>
        /// <returns>An array of section names.</returns>
        public string[] GetSectionNames()
        {
            // Sets the maxsize buffer to 500, if the more is required then doubles the size each time.
            for (int maxsize = 500; true; maxsize *= 2)
            {
                // Obtains the information in bytes and stores them in the maxsize buffer (Bytes array)
                byte[] bytes = new byte[maxsize];
                int size = GetPrivateProfileString(0, "", "", bytes, maxsize, _filePath);

                // Check the information obtained is not bigger than the allocated maxsize buffer - 2 bytes.
                // if it is, then skip over the next section so that the maxsize buffer can be doubled.
                if (size < maxsize - 2)
                {
                    // Converts the bytes value into an ASCII char. This is one long string.
                    string Selected = Encoding.ASCII.GetString(bytes, 0, size - (size > 0 ? 1 : 0));
                    // Splits the Long string into an array based on the "\0" or null (Newline) value and returns the value(s) in an array
                    return Selected.Split(new char[] { '\0' });
                }
            }
        }


        /// <summary>
        /// The Function called to obtain the EntryKey's from the given SectionHeader string passed and returns them in an Dynamic Array.
        /// </summary>
        /// <param name="section"></param>
        /// <returns>An array of entry names.</returns>
        public string[] GetEntryNames(string section)
        {
            // Sets the maxsize buffer to 500, if the more is required then doubles the size each time. 
            for (int maxsize = 500; true; maxsize *= 2)
            {
                // Obtains the EntryKey information in bytes and stores them in the maxsize buffer (Bytes array).
                // Note that the SectionHeader value has been passed.
                byte[] bytes = new byte[maxsize];
                int size = GetPrivateProfileString(section, 0, "", bytes, maxsize, _filePath);

                // Check the information obtained is not biggerthan the allocated maxsize buffer - 2 bytes.
                // if it is, then skip over the next section so that the maxsize buffer can be doubled.
                if (size < maxsize - 2)
                {
                    // Converts the bytes value into an ASCII char. This is one long string.
                    string entries = Encoding.ASCII.GetString(bytes, 0, size - (size > 0 ? 1 : 0));
                    // Splits the Long string into an array based on the "\0" or null (Newline) value and returns the value(s) in an array
                    return entries.Split(new char[] { '\0' });
                }
            }
        }
    }
}
