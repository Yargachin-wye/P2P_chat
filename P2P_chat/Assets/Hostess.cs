using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hostess : MonoBehaviour
{
    [SerializeField] private GameObject _hostessPanel;
    [SerializeField] private GameObject _clienPanel;
    [SerializeField] private GameObject _trackerPanel;
    private void Start()
    {
        _hostessPanel.SetActive(true);
        _clienPanel.SetActive(false);
        _trackerPanel.SetActive(false);
    }
    public void IamATracker()
    {
        _hostessPanel.SetActive(false);
        _clienPanel.SetActive(false);
        _trackerPanel.SetActive(true);
        Tracker.StartReceive();
    }
    public void IamAClient()
    {
        _hostessPanel.SetActive(false);
        _clienPanel.SetActive(true);
        _trackerPanel.SetActive(false);
        ManClientMenager.instance.StartClient();
    }
}
