using GalaSoft.MvvmLight.Command;
using JuegoGato_U3_Jessica_Alondra.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace JuegoGato_U3_Jessica_Alondra
{
    public enum Movimiento { A1, A2, A3, B1, B2, B3, C1, C2, C3 }
    public enum Comando { UsuarioEnviado }
    public class JuegoGato : INotifyPropertyChanged
    {
        public string NombreJugador1 { get; set; } = "Jugador";
        public string NombreJugador2 { get; set; }

        public string IP { get; set; }
        public bool VentanaPrincipalVisible { get; set; } = true;

        public byte PuntosJugador1 { get; set; }
        public byte PuntosJugador2 { get; set; }
        public string MovimientoJugador1 { get; set; }
        public Movimiento? SeleccionJugador1 { get; set; }
        public Movimiento? SeleccionJugador2 { get; set; }
        public string Mensaje { get; set; }

        public ICommand SeleccionarMovimientoCommand { get; set; }
        public ICommand ConfirmarCommand { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        HttpListener servidor;
        ClientWebSocket cliente;
        Dispatcher currentDispatcher;
        public JuegoGato()
        {
            currentDispatcher = Dispatcher.CurrentDispatcher;
            ConfirmarCommand = new RelayCommand<bool>(IniciarPartida);
        }

        private void Lobby_Closing(object sender, CancelEventArgs e)
        {
            VentanaPrincipalVisible = true;
            Actualizar("VentanaPrincipalVisible");
            if (servidor != null)
            {
                servidor.Stop();
                servidor = null;
            }
        }
        Lobby lobby;
        private async void IniciarPartida(bool partida)
        {
            try
            {
                VentanaPrincipalVisible = false;
                lobby = new Lobby();
                lobby.Closing += Lobby_Closing;
                lobby.DataContext = this;
                lobby.Show();
                Actualizar();
                if (partida == true)
                {
                    CrearPartida();
                }
                else
                {
                    NombreJugador2 = NombreJugador1;
                    NombreJugador1 = null;
                    Mensaje = "Intentando conectar con el servidor en " + IP;
                    Actualizar("Mensaje");
                    await ConectarPartida();
                }
            }
            catch (Exception ex)
            {
                Mensaje = ex.Message;
                Actualizar();
            }
        }
        public void CrearPartida()
        {
            servidor = new HttpListener();
            servidor.Prefixes.Add("http://*:1000/gato/");
            servidor.Start();
            servidor.BeginGetContext(OnContext, null);
            Mensaje = "Esperando a conectar con un contrincante...";
            Actualizar();
        }
        public async Task ConectarPartida()
        {
            cliente = new ClientWebSocket();
            await cliente.ConnectAsync(new Uri($"ws://{IP}:1000/gato/"), CancellationToken.None);
            webSocket = cliente;
            RecibirComando();
        }
        WebSocket webSocket;
        private async void OnContext(IAsyncResult ar)
        {
            var context = servidor.EndGetContext(ar);
            if (context.Request.IsWebSocketRequest)
            {
                var listener = await context.AcceptWebSocketAsync(null);
                webSocket = listener.WebSocket;
                CambioMensaje("Cliente aceptado. Esperando la información del contrincante.");
                EnviarComando(new DatoEnviado { Comando = Comando.UsuarioEnviado, Dato = NombreJugador1 });
                RecibirComando();
            }
            else
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
                servidor.BeginGetContext(OnContext, null);
            }
        }
        private async void EnviarComando(DatoEnviado datos)
        {
            byte[] buffer;
            var json = JsonConvert.SerializeObject(datos);
            buffer = Encoding.UTF8.GetBytes(json);
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        private async void RecibirComando()
        {
            byte[] buffer = new byte[1024];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            string datosRecibidos = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var comando = JsonConvert.DeserializeObject<DatoEnviado>(datosRecibidos);
            // Cliente
            if (cliente != null)
            {
                switch (comando.Comando)
                {
                    case Comando.UsuarioEnviado:
                        NombreJugador1 = (string)comando.Dato;
                        CambioMensaje("Conectando con el jugador " + NombreJugador1);
                        EnviarComando(new DatoEnviado { Comando = Comando.UsuarioEnviado, Dato = NombreJugador2 });
                        break;
                }
            }
            // Servidor
            else
            {
                switch (comando.Comando)
                {
                    case Comando.UsuarioEnviado:
                        NombreJugador2 = (string)comando.Dato;
                        CambioMensaje("Conectando con el jugador " + NombreJugador2);
                        break;
                }
            }
        }
        void CambioMensaje(string mensaje)
        {
            currentDispatcher.Invoke(new Action(() =>
            {
                Mensaje = mensaje;
                Actualizar("Mensaje");
            }));
        }
        void Actualizar(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public class DatoEnviado
        {
            public Comando Comando { get; set; }
            public object Dato { get; set; }
        }
    }
}