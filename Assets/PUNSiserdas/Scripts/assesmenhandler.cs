using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class assesmenhandler : MonoBehaviour
{
    int soalno;
    public AudioClip klipbenar;
    public AudioClip klipsalah;
    AudioSource sfx;

    public Text pesan;
    public GameObject Selesai;
    public GameObject senang;
    public GameObject sedih;
    public GameObject spawnerskor;

    public GameObject kembali;
    public Text tulisanskor;
    public Text nilai;
    public int initskor;

    public Text teksSoal;
    public Text[] buttons;
    public Text[] kunci;

    [Header("Default Question Bank")]
    public bool useDefaultQuestionBank = false;
    public int[] kunciIndex;

    [TextArea]
    public string[] soal;
    public string[] buttona;
    public string[] buttonb;
    public string[] buttonc;
    public string[] buttond;
    public string answerstag;

    
    public TMP_InputField inputNama;
    string namaSiswa;
    public List<string> jawabans = new List<string>();

    CountdownTimer ct;

    private void Start()
    {
        ct = FindObjectOfType<CountdownTimer>();
        sfx = this.gameObject.GetComponent<AudioSource>();

        if (ct.paused)
            ct.paused = false;

        EnsureQuestionBank();
        soalno = 0;
        gantisoal();
        initskor = 0;
        nilai.text = initskor.ToString();
    }

    public void diklik()//pilih jawaban
    {
        answerstag = EventSystem.current.currentSelectedGameObject.tag;
        string yangdipilih = EventSystem.current.currentSelectedGameObject.GetComponentInChildren<Text>().text;
        foreach (Text button in buttons)//button didisable biar gak bisa di klik setelah pilih jawaban
            button.transform.parent.GetComponent<Button>().enabled = false;
        if (answerstag == "benar") //kalau jawaban benar 
        {
            answerstag = "salah";
            if (soalno < soal.Length - 1)//jika belum soal terakhir hanya popup.
            {
                senang.SetActive(true);
                StartCoroutine(waittodisable(2.0f, senang));
            }
            
            jawabans.Add("(BENAR) - " + yangdipilih);
            animspawnerskor(10, "++");
            sfx.PlayOneShot(klipbenar);
        }
        else//kalau jawaban salah
        {
            if (soalno < soal.Length - 1)//selagi belum soal terakhir (hanya popup)
            {
                sedih.SetActive(true);
                StartCoroutine(waittodisable(2.0f, sedih));
            }
            jawabans.Add("(SALAH) - " + yangdipilih);
            sfx.PlayOneShot(klipsalah);
        }

        if (soalno < soal.Length - 1)//kalau belum soal terakhir ganti soal
        {
            soalno++;
            gantisoal();
        }
        else  //kalau soal terakhir (mau jawaban terakhir salah ataupun benar)
        {
            ct.paused = true; 

            PlayerData data = new PlayerData(namaSiswa, initskor, jawabans);
            StartCoroutine(SendData(data));

            tulisanskor.text = "Skor Anda = " + initskor.ToString();
            pesan.text = "Selesai";
            Selesai.SetActive(true);
            kembali.SetActive(true);
        }
    }

    public IEnumerator waittodisable(float waittime, GameObject selebrasi)
    {
        yield return new WaitForSeconds(waittime);
        selebrasi.SetActive(false);
        foreach (Text button in buttons)
            button.transform.parent.GetComponent<Button>().enabled = true;
    }

    private void gantisoal()
    {
        for (int x = 0; x < soal.Length; x++)
            if (soalno == x)
            {
                teksSoal.text = soal[x];
                buttons[0].text = buttona[x];
                buttons[1].text = buttonb[x];
                buttons[2].text = buttonc[x];
                buttons[3].text = buttond[x];
                foreach (Text button in buttons)
                    button.transform.parent.tag = "salah";
                if (kunciIndex != null && kunciIndex.Length > x)
                    buttons[kunciIndex[x]].transform.parent.tag = "benar";
                else if (kunci != null && kunci.Length > x && kunci[x] != null)
                    kunci[x].transform.parent.tag = "benar";
            }
    }

    private void EnsureQuestionBank()
    {
        if (!useDefaultQuestionBank) return;

        soal = new string[]
        {
            "Padi adalah serealia yang paling umum diolah menjadi apa?",
            "Jagung termasuk tanaman serealia yang bijinya tumbuh pada bagian mana?",
            "Gandum paling sering diolah menjadi bahan utama produk berikut, kecuali...",
            "Oat dikenal sebagai serealia yang sering dikonsumsi saat sarapan dalam bentuk...",
            "Jelai (barley) umumnya tumbuh baik di daerah dengan iklim...",
            "Sorgum dikenal lebih tahan terhadap kondisi lahan yang...",
            "Milet umumnya memiliki ukuran biji yang...",
            "Jewawut termasuk kelompok serealia dengan bentuk biji yang cenderung...",
            "Jali (Job's tears) dikenal memiliki biji yang...",
            "Kinoa (quinoa) sering dipilih sebagai pangan alternatif karena..."
        };
        buttona = new string[]
        {
            "Nasi", "Tongkol", "Roti", "Oatmeal", "Dingin hingga sejuk",
            "Kering", "Besar", "Lonjong panjang", "Keras dan mengilap", "Kaya protein"
        };
        buttonb = new string[]
        {
            "Tepung", "Daun", "Nasi putih", "Keripik", "Panas dan lembab",
            "Basah", "Kecil", "Bulat kecil", "Lunak dan lembek", "Rendah kalori"
        };
        buttonc = new string[]
        {
            "Minyak", "Akar", "Biskuit", "Bubur manis", "Tropis dan panas",
            "Berbatu", "Sedang", "Gepeng", "Tipis dan ringan", "Bebas gluten"
        };
        buttond = new string[]
        {
            "Gula", "Batang", "Pasta", "Jus buah", "Subtropis kering",
            "Subur dan lembab", "Sangat besar", "Memanjang", "Berwarna merah", "Mudah ditanam"
        };
        kunciIndex = new int[] { 0, 0, 1, 0, 0, 0, 1, 1, 0, 0 };
    }

    private void Update()
    {
        if (ct.timeisup && !Selesai.activeInHierarchy)
        {
            foreach (Text button in buttons)
                button.transform.parent.GetComponent<Button>().enabled = false;
            ct.paused = true;
            Selesai.SetActive(true);
            kembali.SetActive(true);
            tulisanskor.text = "yahh..";
            pesan.text = "Waktu Habis!";
        }
    }

    void animspawnerskor(int addedscore, string teks)
    {
        initskor = initskor + addedscore;
        nilai.text = initskor.ToString();
        spawnerskor.GetComponent<Text>().text = teks;
        spawnerskor.GetComponent<Animation>().Play();
    }

    public void setNamaSiswa()
    {
        namaSiswa = inputNama.text;
        Debug.Log(namaSiswa);
    }

    IEnumerator SendData(PlayerData data)
    {
        string url = "https://script.google.com/macros/s/AKfycbwdFnYxlBEGII1yuucGLJ9Z9YrMfWa_zMlwM5bAWap_AAE-64JwvUu4FGV_YWW4-nQqSw/exec";
        string json = JsonUtility.ToJson(data);
        Debug.Log("Sending: " + json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Success: " + request.downloadHandler.text);
        }
        else
        {
            Debug.Log("Error: " + request.error);
        }
    }

    [System.Serializable]
    public class PlayerData
    {
        public string name;
        public int score;
        public List<string> jawaban = new List<string>(20);

        public PlayerData(string name, int score, List<string> jawaban)
        {
            this.name = name;
            this.score = score;
            this.jawaban = jawaban;
        }
    }
}