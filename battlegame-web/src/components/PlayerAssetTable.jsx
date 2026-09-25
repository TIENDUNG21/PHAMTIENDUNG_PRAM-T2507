export default function PlayerAssetTable({ rows }) {
  if (rows.length === 0) {
    return <p className="message">No data found.</p>;
  }

  return (
    <table className="report-table">
      <thead>
        <tr>
          <th>No</th>
          <th>Player name</th>
          <th>Level</th>
          <th>Age</th>
          <th>Asset name</th>
        </tr>
      </thead>
      <tbody>
        {rows.map((row) => (
          <tr key={row.no}>
            <td>{row.no}</td>
            <td>{row.playerName}</td>
            <td>{row.level}</td>
            <td>{row.age}</td>
            <td>{row.assetName}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
