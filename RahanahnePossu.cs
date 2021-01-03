using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;


/// @author Johanna Virkkunen
/// @version 02.11.2018
/// <summary>
/// Rahanahne possu -peli
/// </summary>
public class RahanahnePossu : PhysicsGame
{
    private const double NOPEUS = 250;
    private const double HYPPYNOPEUS = 750;
    private const int RUUDUN_KOKO = 40;

    private PlatformCharacter pelaaja1;

    private readonly Image pelaajanKuva = LoadImage("possu");
    private readonly Image lantinKuva = LoadImage("lantti");
    private readonly static Image[] verokarhunKuvat = Game.LoadImages("verokarhu", "verokarhu2", "verokarhu3");

    private readonly SoundEffect maaliAani = LoadSoundEffect("maali");

    private IntMeter pisteLaskuri;


    /// <summary>
    /// Begin aliohjema, jossa maaritellaan painovoima, kutsutaan aliohjelmia LuoKentta, LisaaNappaimet ja LuoPistelaskuri.
    /// Maaritetaan kamera seuraamaan pelaajaa, ja kohdistetaan se pelille mielekkaalla tavalla.
    /// </summary>
    public override void Begin()
    {
        Gravity = new Vector(0, -1000);

        LuoKentta();
        LisaaNappaimet();

        Camera.Follow(pelaaja1);
        Camera.ZoomFactor = 1.0;
        Camera.StayInLevel = true;

        MediaPlayer.Play("taustamusiikki");
        MediaPlayer.IsRepeating = true;

        LuoPistelaskuri();
    }


    /// <summary>
    /// Luodaan aliohjelma LuoKentta, jossa kaytetaan kentta1.txt -tiedostoa hyvaksi.
    /// Maaritellaan tietty merkki kutsumaan tiettya aliohjelmaa luomaan haluttu fysiikkaobjekti.
    /// Luodaan rajat seka maaritellaan taustavari.
    /// </summary>
    private void LuoKentta()
    {
         TileMap kentta = TileMap.FromLevelAsset("kentta1");
         kentta.SetTileMethod('#', LisaaTaso);
         kentta.SetTileMethod('*', LisaaLantti);
         kentta.SetTileMethod('N', LisaaPelaaja);
         kentta.SetTileMethod('X', LisaaVerokarhu);
         kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);
         Level.CreateBorders();
         Level.Background.CreateGradient(Color.LightYellow, Color.DarkAzure);
    }


    /// <summary>
    /// Luodaan kultalantti.
    /// </summary>
    /// <param name="paikka">lantin paikka</param>
    /// <param name="leveys">lantin leveys</param>
    /// <param name="korkeus">lantin korkeus</param>
    private void LisaaLantti(Vector paikka, double leveys, double korkeus)
    {
       LuoObjekti(paikka, leveys, korkeus, lantinKuva, "lantti", Color.Gold);
    }


    /// <summary>
    /// Tehdaan tasohyppelytaso viemalla halutut ominaisuudet parametreina LuoObjekti -aliohjelmalle.
    /// </summary>
    /// <param name="paikka">tason paikka</param>
    /// <param name="leveys">tason leveys</param>
    /// <param name="korkeus">tason korkeus</param>
    private void LisaaTaso(Vector paikka, double leveys, double korkeus)
    {
        LuoObjekti(paikka, leveys, korkeus, null, "taso", Color.SeaGreen);
    }


    /// <summary>
    /// Tehdaan yleinen paikallaan pysyva fysiikkaobjekti.
    /// </summary>
    /// <param name="paikka">paikka, johon objekti sijoitetaan</param>
    /// <param name="leveys">objektin leveys</param>
    /// <param name="korkeus">objektin korkeus</param>
    /// <param name="kuva">objektin kuva</param>
    /// <param name="tag">objektin tagi</param>
    /// <param name="vari">objektin vari</param>
    private void LuoObjekti(Vector paikka, double leveys, double korkeus, Image kuva, String tag, Color vari)
    {
      PhysicsObject objekti = PhysicsObject.CreateStaticObject(leveys, korkeus);
      objekti.Image = kuva;
      objekti.Color = vari;
      objekti.Position = paikka;
      objekti.Tag = tag;
      Add(objekti);
    }
    	
    	
	/// <summary>
	/// Luodaan aliohjelma LisaaPelaaja, joka luo uuden PlatformCharacterin pelaaja1.
	/// Annetaan pelaajalle massa seka liitetaan siihen kuvatiedosto.
	/// Lisataan tormayskasittelijoita pelaajan ja lantin -sekä pelaajan ja veroKarhun valille.
	/// </summary>
	/// <param name="paikka">Paikka, johon pelaaja asetetaan</param>
	/// <param name="leveys">Pelaajan leveys</param>
	/// <param name="korkeus">Pelaajan korkeus</param>   
	private void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
	{
	     pelaaja1 = new PlatformCharacter(leveys, korkeus);
	     pelaaja1.Position = paikka;
	     pelaaja1.Mass = 4.0;
	     pelaaja1.Image = pelaajanKuva;
	     AddCollisionHandler(pelaaja1, "lantti", TormaaLanttiin);
	     AddCollisionHandler(pelaaja1, "veroKarhu", PelaajaOsuu);
	     Add(pelaaja1);
	}
	
	
	/// <summary>
	/// Luodaan aliohjelma LisaaVeroKarhu, joka luo uuden PlatformCharacterin vihollinen.
	/// Annetaan viholliselle massa seka liitetaan siihen kuvatiedosto.
	/// Luodaan viholliselle aivot, jotta ne kulkevat edestakaisin tasolla.
	/// </summary>
	/// <param name="paikka">Paikka, johon vihollinen luodaan</param>
	/// <param name="leveys">Vihollisen leveys</param>
	/// <param name="korkeus">Vihollisen korkeus</param>
	private void LisaaVerokarhu(Vector paikka, double leveys, double korkeus)
	{
	     PlatformCharacter vihollinen = new PlatformCharacter(leveys, korkeus);
	     vihollinen.Position = paikka;
	     vihollinen.Mass = 4.0;
	     vihollinen.Image = RandomGen.SelectOne(verokarhunKuvat);
	     vihollinen.Tag = "veroKarhu";
	     Add(vihollinen);
	     
	     PlatformWandererBrain tasoAivot = new PlatformWandererBrain();
	     tasoAivot.Speed = 100;
	     vihollinen.Brain = tasoAivot;
	}
	
	
	/// <summary>
	/// Lisataan nappainkuuntelijoita.
	/// </summary>
	private void LisaaNappaimet()
	{
	     Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
	     Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
	     
	     Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, -NOPEUS);
	     Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, NOPEUS);
         Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, HYPPYNOPEUS);
    }

    
    /// <summary>
    /// Luodaan aliohjelma Liikuta, jota kutsuttaessa hahmo liikkuu oikealle/vasemmalle nappaimia painaessa.
    /// </summary>
    /// <param name="hahmo">Hahmo, jota liikutetaan</param>
    /// <param name="nopeus">Hahmon nopeus</param>
    private void Liikuta(PlatformCharacter hahmo, double nopeus)
    {
         hahmo.Walk(nopeus);
    }


    /// <summary>
    /// Luodaan aliohjelma Hyppaa, jota kutsuttaessa hahmo hyppaa ylospain.
    /// </summary>
    /// <param name="hahmo">Hahmo, jota liikutetaan</param>
    /// <param name="nopeus">Hahmon nopeus</param>
    private void Hyppaa(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Jump(nopeus);
    }


    /// <summary>
    /// Luodaan aliohjelma TormaaLanttiin, joka aiheuttaa aaniiefektin, lisaa nayttoon viestin seka kasvattaa pistelaskurin arvoa.
    /// </summary>
    /// <param name="hahmo">Hahmo, joka osuu lanttiin</param>
    /// <param name="lantti">Lantti, johon hahmo osuu</param>
    private void TormaaLanttiin(PhysicsObject hahmo, PhysicsObject lantti)
    {
         maaliAani.Play();
         MessageDisplay.Add("Röhröh sain lantin!");
         pisteLaskuri.Value += 1;
         lantti.Destroy();
    }


    /// <summary>
    /// Luodaan aliohjelma LuoPisteLaskuri, joka luo nayttoon laskurin, joka laskee kerattyjen lanttien maaran.
    /// </summary>
    private void LuoPistelaskuri()
    {
         pisteLaskuri = new IntMeter(0);
         Label pisteNaytto = new Label();
         pisteNaytto.X = Screen.Right - 50;
         pisteNaytto.Y = Screen.Top - 50;
         pisteNaytto.TextColor = Color.Black;
         pisteNaytto.Color = Color.Gold;
         pisteNaytto.BindTo(pisteLaskuri);
         pisteNaytto.Title = "Lantit";
         pisteLaskuri.MaxValue = 15;
         pisteLaskuri.UpperLimit += KaikkiKeratty;
         Add(pisteNaytto);
    }


    /// <summary>
    /// Luodaan aliohjelma KaikkiKeratty, joka luo kultahippusateen laskurin saavuttaessa maksimiarvonsa.
    /// </summary>
    private void KaikkiKeratty()
    {
         ClearAll();
         
         Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
         
         Level.Background.CreateGradient(Color.White, Color.Gold);
         
         MessageDisplay.Add("Onnistuit keräämään kaikki lantit jäämättä kiinni verokarhulle! Paina Esc poistuaksesi.");
         
         Gravity = new Vector(0, -500);
         int i = 0;
         int maara = 2000;
         while (i<maara)
         {
              int sade = RandomGen.NextInt(20, 20);
              double x = RandomGen.NextDouble(Level.Left + sade, Level.Right - sade);
              double y = RandomGen.NextDouble(Level.Bottom + sade, Level.Top - sade);
              Color vari = Color.Gold;
              PhysicsObject hippu = LuoHippu(x, y, vari, sade);
              Add(hippu, 1);
              i++;
         }   
    }
   

    /// <summary>
    /// Luodaan aliohjelma LuoHippu, joka luo uuden ympyran muotoisen fysiikkaobjektin halutuilla ominaisuuksilla.
    /// </summary>
    /// <param name="x">hipun x-koordinaatti</param>
    /// <param name="y">hipun y-koordinaatti</param>
    /// <param name="vari">hipun vari</param>
    /// <param name="sade">hipun sade</param>
    /// <returns>hippu</returns>
    private PhysicsObject LuoHippu(double x, double y, Color vari, double sade)
    {
         PhysicsObject hippu = new PhysicsObject(2 * sade, 2 * sade, Shape.Circle);
         hippu.Color = vari;
         hippu.X = x;
         hippu.Y = y;
         return hippu;
    }


    /// <summary>
    /// Luodaan aliohjelma PelaajaOsuu, joka pelaajan ja vihollisen tormatessa aiheuttaa viestin nayttoon seka vahentaa pelaajan terveytta -1 verran.
    /// Lisataan ehtolause, jossa pelaajan terveyden mennessa nollaan, se tuhoutuu.
    /// </summary>
    /// <param name="pelaaja1">Pelaaja, joka osuu viholliseen</param>
    /// <param name="vihollinen">Vihollinen eli verokarhu, johon osutaan</param>
    private void PelaajaOsuu(PhysicsObject pelaaja1, PhysicsObject vihollinen)
    {
         ClearAll();
         
         Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
         
         pelaaja1.Destroy();
         
         Level.Background.CreateGradient(Color.Black, Color.BloodRed);
         
         Label tekstikentta = new Label(800.0, 400.0, "Jouduit verokarhun hampaisiin ja hävisit pelin! Paina Esc poistuaksesi pelistä.");
         tekstikentta.Color = Color.MidnightBlue;
         tekstikentta.TextColor = Color.White;
         tekstikentta.BorderColor = Color.Black;
         Add(tekstikentta);
    } 


}
