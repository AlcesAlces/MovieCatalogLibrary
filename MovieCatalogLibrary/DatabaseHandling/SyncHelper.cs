using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieCatalogLibrary.DatabaseHandling
{
    public class SyncHelper
    {
        private int groupSize { get; set; }
        public static Client socket { get; set; }

        public SyncHelper(Client sock, int gsize = 10)
        {
            this.groupSize = gsize;
            socket = sock;
        }
        public void StartSync(FileHandler.platformType platform = FileHandler.platformType.Windows)
        {
            FileHandler handler = new FileHandler();
            List<Movie> movies = handler.allMoviesInXml();
            List<List<Movie>> toSend = DivideByAmt(this.groupSize, movies);
            CreateLink(toSend);
        }

        private void CreateLink(List<List<Movie>> toSend)
        {
            //new { user = user.ToLower(), password = pass.ToLower(), sync = false });
            socket.Emit("link", new { size = this.groupSize });

            socket.On("link_success", (fn) =>
            {
                SyncLoop(toSend);
            });
        }

        private void SyncLoop(List<List<Movie>> movieLoop)
        {
            //TODO: Add timeout logic.
            //Clear link_success so it's a once evaluation.
            socket.On("link_success", (fn) => { });
            int i = 0;

            socket.On("burst_recieved", (fn) =>
            {
                //TODO: Signal the UI component.
                i++;
                if(i == movieLoop.Count)
                {
                    socket.On("burst_recieved", (fnd) => {});
                    //TODO: Meta Information?
                    socket.Emit("burst_complete", "complete");
                    Done();
                }
                else
                {
                    socket.Emit("burst_send", new { burstNo = i, payload = movieLoop.ElementAt(i) });
                }
            });

            //Starts the communication loop.
            socket.Emit("burst_send", new { burstNo = i, payload = movieLoop.ElementAt(i) });
        }

        private void Done()
        {
            //TODO: Uhm?
        }

        private List<List<Movie>> DivideByAmt(int size, List<Movie> allMovies)
        {
            List<List<Movie>> toReturn = new List<List<Movie>>();

            int count = 0;
            toReturn.Add(new List<Movie>());
            foreach(Movie item in allMovies)
            {
                if (count < size)
                {
                    toReturn.LastOrDefault().Add(item);
                    count++;
                }
                else
                {
                    toReturn.Add(new List<Movie>());
                    toReturn.LastOrDefault().Add(item);
                    count = 1;
                }
            }

            return toReturn;
        }

    }
}
