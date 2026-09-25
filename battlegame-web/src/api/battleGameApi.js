const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:7071/api';

/**
 * Calls the getassetsbyplayer Azure Function.
 * @param {string} [playerName] optional filter by player name
 */
export async function getAssetsByPlayer(playerName) {
  const query = playerName ? `?playerName=${encodeURIComponent(playerName)}` : '';
  const response = await fetch(`${API_BASE_URL}/getassetsbyplayer${query}`);

  if (!response.ok) {
    throw new Error(`Request failed with status ${response.status}`);
  }

  return response.json();
}
