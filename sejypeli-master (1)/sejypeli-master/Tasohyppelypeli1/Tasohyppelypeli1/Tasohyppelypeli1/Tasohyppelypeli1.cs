using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Effects;
using Jypeli.Widgets;

public class Tasohyppelypeli1 : PhysicsGame
{
    const double nopeus = 200;
    const double hyppyNopeus = 750;
    const int RUUDUN_KOKO = 40;

    PlatformCharacter pelaaja1;
    IntMeter pelastetut;
    

    PhysicsObject alaReuna;
    Image pelaajanKuva = LoadImage("SuS");
    Image tahtiKuva = LoadImage("immeinen");
    Image fedoraKuva = LoadImage("tipsfedora");
    Image lootanKuva = LoadImage("crate");
    Image palkinKuva = LoadImage("girder");
    Image kieltajanKuva = LoadImage("kieltaja");

    SoundEffect maaliAani = LoadSoundEffect("maali");
    Image taustaKuva = LoadImage("katukuva");

    public override void Begin()
    {
        Gravity = new Vector(0, -1000);

        LuoKentta();
        LisaaNappaimet();
        LuoPisteLaskuri();

        Camera.Follow(pelaaja1);
        Camera.ZoomFactor = 1.2;
        Camera.StayInLevel = true;
    }

    void LuoKentta()
    {
        TileMap kentta = TileMap.FromLevelAsset("kentta1");
        kentta.SetTileMethod('#', LisaaTaso);
        kentta.SetTileMethod('X', LisaaLoota);
        kentta.SetTileMethod('1', LisaaPalkki);
        kentta.SetTileMethod('*', LisaaTahti);
        kentta.SetTileMethod('N', LisaaPelaaja);
        kentta.SetTileMethod('F', LisaaFedora);
        kentta.SetTileMethod('K', LisaaKieltaja);
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);
        Level.Background.Image = (taustaKuva);
        Level.Background.TileToLevel();
        alaReuna = Level.CreateBottomBorder();
        alaReuna.IsVisible = false;
        alaReuna.Tag = "alareuna";

      
    }

    void LisaaTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Color = Color.DarkGray;
        Add(taso);
    }

    void LisaaLoota(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject loota = PhysicsObject.CreateStaticObject(leveys, korkeus);
        loota.Position = paikka;
        loota.Image = lootanKuva;
        Add(loota);
    }

    void LisaaPalkki(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject palkki = PhysicsObject.CreateStaticObject(leveys, korkeus);
        palkki.Position = paikka;
        palkki.Image = palkinKuva;
        Add(palkki);
    }

    void LisaaKieltaja(Vector paikka, double leveys, double korkeus)
    {
        PlatformCharacter kieltaja = new PlatformCharacter(30, 70);
        kieltaja.Position = paikka;
        kieltaja.Tag = "kieltaja";
        kieltaja.Image = kieltajanKuva;
        Add(kieltaja);

        PlatformWandererBrain tasoAivot = new PlatformWandererBrain();
        tasoAivot.Speed = 100;
        kieltaja.Brain = tasoAivot;

    }

    void LisaaTahti(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject tahti = PhysicsObject.CreateStaticObject(30, 70);
        tahti.IgnoresCollisionResponse = true;
        tahti.Position = paikka;
        tahti.Image = tahtiKuva;
        tahti.Tag = "tahti";
        Add(tahti);
    }
    void LisaaFedora(Vector paikka, double leveys, double korkeus)
    {
        PlatformCharacter fedora = new PlatformCharacter(45, 70);
        fedora.Mass = 10.0;
        fedora.Position = paikka;
        fedora.Image = fedoraKuva;
        Add(fedora);
    }

    void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
    {
        pelaaja1 = new PlatformCharacter(30, 70);
        pelaaja1.Position = paikka;
        pelaaja1.Mass = 4.0;
        pelaaja1.Image = pelaajanKuva;
        AddCollisionHandler(pelaaja1, "tahti", TormaaTahteen);
        Add(pelaaja1);
        AddCollisionHandler(pelaaja1, "alareuna", Tippuminen);
        AddCollisionHandler(pelaaja1, "kieltaja", Kuoleminen);
        pelaaja1.IgnoresExplosions = true;
    }

    void LisaaNappaimet()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, -nopeus);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, nopeus);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, hyppyNopeus);
        Keyboard.Listen(Key.X, ButtonState.Pressed, Splosion, "Räjähdys");
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
    }

    void Liikuta(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Walk(nopeus);
    }

    void Hyppaa(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Jump(nopeus);
    }
    void Tippuminen(PhysicsObject pelaaja1, PhysicsObject alaReuna)
    {
        AloitaAlusta();
    }
    void Kuoleminen(PhysicsObject pelaaja1, PhysicsObject kieltaja)
    {
        AloitaAlusta();
    }
    void Splosion()
    {
        Explosion rajahdys = new Explosion(50);
        rajahdys.Position = pelaaja1.Position;
        rajahdys.Speed = 100.0;
        rajahdys.Force = 10000;
        rajahdys.ShockwaveColor = new Color(256, 0, 256, 70);
        rajahdys.AddShockwaveHandler("kieltaja", PaineaaltoOsuu);
        Add(rajahdys);
    }
    void PaineaaltoOsuu(IPhysicsObject kieltaja, Vector shokki)
    {
        kieltaja.Destroy();
    }

    void TormaaTahteen(PhysicsObject hahmo, PhysicsObject tahti)
    {
        maaliAani.Play();
        MessageDisplay.Add("SuS pelastaa!");
        tahti.Destroy();
        pelastetut.Value += 1;
        if (pelastetut.Value == pelastetut.MaxValue)
        {
            KaikkiKeratty();
        }
    }
    void LuoPisteLaskuri()
    {
       pelastetut = new IntMeter(0);
       
    
    }
    void KaikkiKeratty()
    {

    }
    void AloitaAlusta()
    {
        
        ClearAll();
        LuoKentta();
        LisaaNappaimet();
        Camera.Follow(pelaaja1);
        Camera.ZoomFactor = 1.2;
        Camera.StayInLevel = true;
        Gravity = new Vector(0, -1000);
     
    }

}