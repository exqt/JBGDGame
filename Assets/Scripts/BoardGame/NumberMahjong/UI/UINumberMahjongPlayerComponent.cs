using System.Security;
using UnityEngine;
using UnityEngine.UIElements;

public partial class UINumberMahjongRoundResult {
    public class PlayerComponent {
        static Color[] RANK_COLORS = new Color[] {
            // golden
            new(1, 0.8431373f, 0),
            // silver
            new(0.7529412f, 0.7529412f, 0.7529412f),
            // bronze
            new(0.8f, 0.4980392f, 0.1960784f),
            // warning
            new(0.9f, 0.6f, 0.6f),
        };

        VisualElement root;
        VisualElement playerImage;
        private Label ranking;
        private Label playerName;
        private Label currentScore;
        private Label scoreChange;

        public string PlayerName { set => playerName.text = value; }
        public int Ranking {
            set {
                ranking.text = $"#{value}";
                ranking.style.backgroundColor = RANK_COLORS[value - 1];
            }
        }

        public int CurrentScore {
            set { currentScore.text = $"{value,3} Pts"; }
        }

        public int ScoreChange {
            set {
                if (value == 0) {
                    scoreChange.text = "";
                    return;
                }

                var signStr = value >= 0 ? "+" : "-";
                if (signStr == "-") scoreChange.style.color = new Color(1, 0, 0);
                else scoreChange.style.color = new Color(0, 1, 0);

                string valueStr = Mathf.Abs(value).ToString().PadLeft(2, ' ');
                scoreChange.text = $"({signStr}{valueStr})";
            }
        }

        public void SetImage(Texture2D texture) {
            var backgroundImage = new StyleBackground(texture);
            playerImage.style.backgroundImage = backgroundImage;
        }

        public void SetName(string name) {
            playerName.text = name;
        }

        public PlayerComponent(VisualElement wrapper) {
            root = wrapper.Q<VisualElement>("root");
            playerImage = root.Q<VisualElement>("PlayerImage");
            playerName = root.Q<Label>("PlayerName");
            ranking = root.Q<Label>("Ranking");
            currentScore = root.Q<Label>("CurrentScore");
            scoreChange = root.Q<Label>("ScoreChange");
        }
    }
}
