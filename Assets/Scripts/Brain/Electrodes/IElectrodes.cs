using System.Collections.Generic;

public interface IElectrodes
{
    List<object> electrodes { get; }
    int loadPtsFile(string pathPts);
    void loadElectrodesOnBrain();
    void updateElectrodesPosition();
    void loadAtlasData(string pathAtlasCsv);
    void loadDefaultPearl(ELAN[] elanFiles);
    void updateElectrodesPearl();
}
