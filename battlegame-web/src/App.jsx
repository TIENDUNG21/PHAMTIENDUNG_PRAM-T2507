import { useCallback, useEffect, useState } from 'react';
import { getAssetsByPlayer } from './api/battleGameApi.js';
import PlayerAssetTable from './components/PlayerAssetTable.jsx';

export default function App() {
  const [rows, setRows] = useState([]);
  const [playerName, setPlayerName] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const loadReport = useCallback(async (filter) => {
    setIsLoading(true);
    setError('');
    try {
      setRows(await getAssetsByPlayer(filter));
    } catch (err) {
      setError(`Cannot load report: ${err.message}`);
      setRows([]);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    loadReport();
  }, [loadReport]);

  const handleSearch = (event) => {
    event.preventDefault();
    loadReport(playerName.trim());
  };

  const handleReset = () => {
    setPlayerName('');
    loadReport();
  };

  return (
    <main className="container">
      <h1>Battle Game</h1>
      <h2>Player Assets Report</h2>

      <form className="search-bar" onSubmit={handleSearch}>
        <input
          type="text"
          placeholder="Filter by player name..."
          value={playerName}
          onChange={(event) => setPlayerName(event.target.value)}
        />
        <button type="submit">Search</button>
        <button type="button" className="secondary" onClick={handleReset}>
          Reset
        </button>
      </form>

      {isLoading && <p className="message">Loading...</p>}
      {error && <p className="message error">{error}</p>}
      {!isLoading && !error && <PlayerAssetTable rows={rows} />}
    </main>
  );
}
